using System.Security.Cryptography;
using Platform.Text;
using Platform.Utilities;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams.Interactive;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("ADHOCENCRYPTION", HandshakeRunLevel.NAME)]
	public class AdhocEncryptionCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption(Required = false)]
			public bool Compress = false;

			[CommandLineOption(Required = false)]
			public string Modulus = null;

			[CommandLineOption(Required = false)]
			public string Exponent = null;

			[CommandLineOption(Required = false)]
			public string Algorithm = "aes";

			[CommandLineOption("w", Required = false)]
			public bool WaitForReady = false;
		}

		public AdhocEncryptionCommandProcessor(Connection connection)
			: base(connection, typeof(CommandOptions))
		{
		}

		public override void Process(Command command)
		{
			var options = this.LoadOptions<CommandOptions>((TextCommand)command);

			if (options.Modulus != null && options.Exponent != null && options.Algorithm != null)
			{
				var symmetric = new RijndaelManaged();
				symmetric.GenerateKey();

				var modulus = TextConversion.FromBase64String(options.Modulus);
				var exponent = TextConversion.FromBase64String(options.Exponent);

				var rsaparameters = new RSAParameters();
				rsaparameters.Modulus = modulus;
				rsaparameters.Exponent = exponent;
				
				var rsa = new RSACryptoServiceProvider();
				rsa.ImportParameters(rsaparameters);
				
				var myrsa = new RSACryptoServiceProvider();

				rsaparameters = myrsa.ExportParameters(false);

				Connection.WriteOk
				(
					"Modulus",
					TextConversion.ToBase64String(rsaparameters.Modulus),
					"Exponent",
					TextConversion.ToBase64String(rsaparameters.Exponent),
					"IV",
					TextConversion.ToBase64String(new RSAPKCS1KeyExchangeFormatter(rsa).CreateKeyExchange(symmetric.IV)),
					"KeyExchange",
					TextConversion.ToBase64String(new RSAPKCS1KeyExchangeFormatter(rsa).CreateKeyExchange(symmetric.Key))
				);

				Connection.Flush();

				if (options.WaitForReady)
				{
					Connection.ReadReady();
				}
			}
			else if (options.Compress)
			{
				Connection.WriteOk();
				Connection.Flush();

				this.Connection.ReadStream = new InteractiveInflaterInputStream(this.Connection.RawReadStream, new Inflater(true));
				this.Connection.WriteStream = new InteractiveDeflaterOutputStream(this.Connection.RawWriteStream, new Deflater(Deflater.DEFAULT_COMPRESSION, true), 512);

				if (options.WaitForReady)
				{
					Connection.ReadReady();
				}
			}
		}
	}
}
