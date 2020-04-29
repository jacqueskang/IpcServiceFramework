## Stream translators

If you want to process the binary data after serialisation or before deserialisation, for example to add a custom handshake when the connection begins, you can do so using a stream translator. Host and client classes allow you to pass a `Func<Stream, Stream>` stream translation callback in their constructors, which can be used to "wrap" a custom stream around the network stream. This is supported on TCP communications both with and without SSL enabled. See the `XorStream` class in the IpcServiceSample.ServiceContracts project for an example of a stream translator.

Stream translators are also useful for logging packets for debugging. See the `LoggingStream` class in the IpcServiceSample.ServiceContracts project for an example of using a stream translator to log traffic.
