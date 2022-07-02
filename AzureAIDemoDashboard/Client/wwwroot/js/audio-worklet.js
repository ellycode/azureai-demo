class MyAudioWorklet extends AudioWorkletProcessor {
    constructor(...args) {
        super(...args);
    }

    process(inputs, outputs, parameters) {
        let inputData = inputs[0][0];
        if (inputData) {
            this.port.postMessage({ inputData });
        }
        return true
    }
}

registerProcessor('audio-worklet', MyAudioWorklet);
