// original source code is taken from:
// https://github.com/SimpleWebRTC/hark
// copyright goes to &yet team
// edited by Muaz Khan for RTCMultiConnection.js
// also edited by @meronz

// taken from the amazon-lex sdk
function downsampleBuffer(buffer, recordSampleRate, exportSampleRate) {
    if (exportSampleRate === recordSampleRate) {
        return buffer;
    }
    var sampleRateRatio = recordSampleRate / exportSampleRate;
    var newLength = Math.round(buffer.length / sampleRateRatio);
    var result = new Float32Array(newLength);
    var offsetResult = 0;
    var offsetBuffer = 0;
    while (offsetResult < result.length) {
        var nextOffsetBuffer = Math.round((offsetResult + 1) * sampleRateRatio);
        var accum = 0,
            count = 0;
        for (var i = offsetBuffer; i < nextOffsetBuffer && i < buffer.length; i++) {
            accum += buffer[i];
            count++;
        }
        result[offsetResult] = accum / count;
        offsetResult++;
        offsetBuffer = nextOffsetBuffer;
    }
    return result;
}

// taken from the amazon-lex sdk
function mergeBuffers(bufferArray, recLength) {
    var result = new Float32Array(recLength);
    var offset = 0;
    for (var i = 0; i < bufferArray.length; i++) {
        result.set(bufferArray[i], offset);
        offset += bufferArray[i].length;
    }
    return result;
}

// taken from the amazon-lex sdk
function floatTo16BitPCM(output, offset, input) {
    for (var i = 0; i < input.length; i++, offset += 2) {
        var s = Math.max(-1, Math.min(1, input[i]));
        output.setInt16(offset, s < 0 ? s * 0x8000 : s * 0x7FFF, true);
    }
}

// taken from the amazon-lex sdk
function writeString(view, offset, string) {
    for (var i = 0; i < string.length; i++) {
        view.setUint8(offset + i, string.charCodeAt(i));
    }
}

// taken from the amazon-lex sdk
function encodeWAV(samples, sampleRate) {
    var buffer = new ArrayBuffer(44 + samples.length * 2);
    var view = new DataView(buffer);

    writeString(view, 0, 'RIFF');
    view.setUint32(4, 32 + samples.length * 2, true);
    writeString(view, 8, 'WAVE');
    writeString(view, 12, 'fmt ');
    view.setUint32(16, 16, true);
    view.setUint16(20, 1, true);
    view.setUint16(22, 1, true);
    view.setUint32(24, sampleRate, true);
    view.setUint32(28, sampleRate * 2, true);
    view.setUint16(32, 2, true);
    view.setUint16(34, 16, true);
    writeString(view, 36, 'data');
    view.setUint32(40, samples.length * 2, true);
    floatTo16BitPCM(view, 44, samples);

    return view;
}

// taken from the amazon-lex sdk
export function exportWAV(recArray, recLength, recordSampleRate, exportSampleRate) {
    exportSampleRate = exportSampleRate || recordSampleRate;

    var mergedBuffers = mergeBuffers(recArray, recLength);
    var downsampledBuffer = downsampleBuffer(mergedBuffers, recordSampleRate, exportSampleRate);
    var encodedWav = encodeWAV(downsampledBuffer, exportSampleRate);
    return new Blob([encodedWav], { type: 'media/wav' });
}

export function hark(stream, options) {
    var audioContextConstructor = window.webkitAudioContext || window.AudioContext;
    var harker = this;

    harker.enabled = false;
    harker.events = {};
    harker.audioDataBuffer = [];
    harker.audioDataBufferLen = 0;
    harker.on = function (event, callback) {
        harker.events[event] = callback;
    };

    harker.emit = function () {
        if (harker.enabled && harker.events[arguments[0]]) {
            harker.events[arguments[0]](arguments[1], arguments[2], arguments[3], arguments[4]);
        }
    };

    // make it not break in non-supported browsers
    if (!audioContextConstructor) return harker;

    options = options || {};
    // Config
    var smoothing = (options.smoothing || 0.1),
        interval = (options.interval || 50),
        threshold = options.threshold,
        play = options.play,
        history = options.history || 10,
        running = true;

    // Setup Audio Context
    threshold = threshold || -50;
    harker.speaking = false;
    harker.fftSize = 2048;
    const fftBins = new Float32Array(harker.fftSize);
    
    let audioContext = new audioContextConstructor();
    harker.sampleRate = audioContext.sampleRate;
    
    const sourceNode = audioContext.createMediaStreamSource(stream);
    const analyser = audioContext.createAnalyser();
    analyser.fftSize = harker.fftSize;
    analyser.smoothingTimeConstant = smoothing;
    
    audioContext.audioWorklet.addModule(`js/audio-worklet.js?c=${Date.now()}`).then(() => {
        const audioWorkletNode = new AudioWorkletNode(audioContext, 'audio-worklet');
        audioWorkletNode.port.onmessage = function (event) {
            if(!harker.enabled) return;

            let audioData = event.data.inputData;
            harker.audioDataBuffer.push(audioData);
            harker.audioDataBufferLen += audioData.length;

            if (!harker.speaking) {
                if (harker.audioDataBufferLen >= harker.sampleRate) {
                    let discardedBuff = harker.audioDataBuffer.shift();
                    harker.audioDataBufferLen -= discardedBuff.length;
                }
            }
        };
        sourceNode.connect(analyser);
        analyser.connect(audioWorkletNode);
        audioWorkletNode.connect(audioContext.destination);
    });

    harker.setThreshold = function (t) {
        threshold = t;
    };

    harker.setInterval = function (i) {
        interval = i;
    };

    harker.stop = function () {
        running = false;
        harker.emit('volume_change', -100, threshold);
        if (harker.speaking) {
            harker.speaking = false;
            harker.emit('stopped_speaking');
        }
    };
    
    harker.clean = function () {
        harker.audioDataBufferLen = 0;
        harker.audioDataBuffer = [];
        harker.speakingHistory = [];
        for (var i = 0; i < history; i++) {
            harker.speakingHistory.push(0);
        }
    };
    
    harker.speakingHistory = [];
    for (var i = 0; i < history; i++) {
        harker.speakingHistory.push(0);
    }

    // Poll the analyser node to determine if speaking
    // and emit events if changed
    var looper = function () {
        setTimeout(function () {
            //check if stop has been called
            if (!running) {
                return;
            }

            var currentVolume = getMaxVolume(analyser, fftBins);

            harker.emit('volume_change', currentVolume, threshold);

            var history = 0;
            if (currentVolume > threshold && !harker.speaking) {
                // trigger quickly, short history
                for (var i = harker.speakingHistory.length - 3; i < harker.speakingHistory.length; i++) {
                    history += harker.speakingHistory[i];
                }
                if (history >= 2) {
                    harker.speaking = true;
                    harker.emit('speaking');
                }
            } else if (currentVolume < threshold && harker.speaking) {
                for (var j = 0; j < harker.speakingHistory.length; j++) {
                    history += harker.speakingHistory[j];
                }
                if (history === 0) {
                    harker.speaking = false;
                    harker.emit('stopped_speaking');
                }
            }
            harker.speakingHistory.shift();
            harker.speakingHistory.push(0 + (currentVolume > threshold));

            looper();
        }, interval);
    };
    looper();

    function getMaxVolume(analyser, fftBins) {
        var maxVolume = -Infinity;
        analyser.getFloatFrequencyData(fftBins);

        for (var i = 4, ii = fftBins.length; i < ii; i++) {
            if (fftBins[i] > maxVolume && fftBins[i] < 0) {
                maxVolume = fftBins[i];
            }
        }

        return maxVolume;
    }

    harker.exportBlobWAV = function() {
        let bufferLen = harker.audioDataBufferLen;
        harker.audioDataBufferLen = 0;
        let buffer = harker.audioDataBuffer.splice(0, harker.audioDataBuffer.length);
        return exportWAV(buffer, bufferLen, harker.sampleRate, 16000);
    }

    return harker;
}