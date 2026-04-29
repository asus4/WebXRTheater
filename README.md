# Unity x WebXR Experiment

A prototype for a WebXR using Unity.

https://github.com/user-attachments/assets/63802f85-6eb0-4d0a-8a27-f0a29f32910b

## Tested device

- Meta Quest 3: The OS default Browser

## How to develop

```sh
# Run the local server which supports Brotli files for Unity WebGL
npm run serve

# Run cloudflared or ngrok if you want to test on a device, such as Meta Quest.
cloudflared tunnel --url http://localhost:8080
```

## Third Party Licenses

- [De-Panther/unity-webxr-export](https://github.com/De-Panther/unity-webxr-export): Apache License 2.0
- [Share Tech Mono Font](https://fonts.google.com/specimen/Share+Tech+Mono/license?preview.script=Latn): SIL Open Font License 1.1
