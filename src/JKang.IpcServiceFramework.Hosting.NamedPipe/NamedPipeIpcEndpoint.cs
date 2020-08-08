using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace JKang.IpcServiceFramework.Hosting.NamedPipe
{
    public class NamedPipeIpcEndpoint<TContract> : IpcEndpoint<TContract>
        where TContract : class
    {
        private readonly NamedPipeIpcEndpointOptions _options;

        public NamedPipeIpcEndpoint(
            NamedPipeIpcEndpointOptions options,
            ILogger<NamedPipeIpcEndpoint<TContract>> logger,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider, logger)
        {
            _options = options;
        }

        protected override async Task WaitAndProcessAsync(
            Func<Stream, CancellationToken, Task> process,
            CancellationToken cancellationToken)
        {
            if (process is null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            // https://github.com/PowerShell/PowerShellEditorServices/blob/f45c6312a859cde4aa25ea347a345e1d35238350/src/PowerShellEditorServices.Protocol/MessageProtocol/Channel/NamedPipeServerListener.cs#L38-L67
            // Unfortunately, .NET Core does not support passing in a PipeSecurity object into the constructor for
            // NamedPipeServerStream so we are creating native Named Pipes and securing them using native APIs.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PipeSecurity pipeSecurity = new PipeSecurity();
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                PipeAccessRule psRule = new PipeAccessRule(everyone, PipeAccessRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);
                pipeSecurity.AddAccessRule(psRule);
                using (var server = NamedPipeNative.CreateNamedPipe(_options.PipeName, (uint) _options.MaxConcurrentCalls, pipeSecurity))
                {
                    await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                    await process(server, cancellationToken).ConfigureAwait(false);
                }
            }

            // Use original logic on other platforms.
            else
            {
                using (var server = new NamedPipeServerStream(_options.PipeName, PipeDirection.InOut, _options.MaxConcurrentCalls,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                    await process(server, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
