import { hark } from './hark.js';
let gThisRef = null;
let gHarker = null;
let gStream = null;
let gLocale = null;

export async function init(thisRef, locale) {
    gThisRef = thisRef;
    gLocale = locale;
    
    gStream = await navigator.mediaDevices.getUserMedia({audio: true});
    gHarker = new hark(gStream);
    gHarker.enabled = false;

    return true;
}

export function startRecord() {
    gHarker.enabled = true;
    gHarker.on('stopped_speaking', () => {
        let blob = gHarker.exportBlobWAV();
        let data = new FormData();
        data.append('file', blob, 'audio.wav');
        fetch(`/speech/recognizeaudio?locale=${gLocale}`,
            {
                method: 'post',
                body: data
            })
            .then(result => {
                result.json().then(data => {
                    gThisRef.invokeMethod(
                        'ShowResult', data);
                });
            });
    });
}

export async function synthesizeText(request) {
    let response = await fetch('/speech/synthesizetext',
        {
            method: 'post',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(request)
        });
    let audioData = await response.blob();
    let audioPlayer = document.querySelector("audio");
    audioPlayer.src = URL.createObjectURL(audioData);
    
    let harkerState = gHarker.enabled;
    gHarker.enabled = false;
    gHarker.clean();
    audioPlayer.onended = function() {
        gHarker.enabled = harkerState;
    }

    await audioPlayer.play();
}

export async function stop() {
    gHarker.enabled = false;
    gHarker.clean();
}