using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;

namespace InfiniteSpace.Framework.Build.Tasks
{
	/// <summary>
	/// Take the .png files in the Assets and produce a .etc1 and .alpha files for the same name
	/// </summary>
	public class CompressTextures : Task
	{
		[Required]
		public ITaskItem[] InputFiles { get; set; }

		[Required]
		public string AndroidSdkDir { get; set; }

		[Output]
		public ITaskItem[] OutputFiles { get; set; }

		/// <Docs>To be added.</Docs>
		/// <summary>
		/// for each taskitem. 
		///    If its a .png file split out the alpha 
		///      then run the file though the etc1tool to generate an etc1 texture.
		///      add both items to the OutputFiles list, but not the origional
		///    If its not a .png just add the item to the OutputFileslist
		/// </summary>
		public override bool Execute ()
		{
			Log.LogMessage (MessageImportance.Low, "  CompressTextures Task");

			List<ITaskItem> items = new List<ITaskItem> ();
			var etc1tool = new Etc1Tool ();
			etc1tool.AndroidSdkDir = AndroidSdkDir;

			foreach (var item in InputFiles) {
				if (item.ItemSpec.Contains(".png")) {
					var etc1file = item.ItemSpec.Replace (".png", ".etc1");
					var alphafile = item.ItemSpec.Replace (".png", ".alpha");
					byte[] data = null;

					using (var bitmap = (Bitmap)Bitmap.FromFile (item.ItemSpec)) {
						data = new byte[bitmap.Width * bitmap.Height];
						for (int y = 0; y < bitmap.Height; y++) {
							for (int x = 0; x < bitmap.Width; x++) {
								var color = bitmap.GetPixel (x, y);
								data [(y * bitmap.Width) + x] = color.A;
							}
						}
					}
						
					if (data != null)
						File.WriteAllBytes (alphafile, data);

					// generate etc1 image
					etc1tool.Source = item.ItemSpec;
					etc1tool.Destination = etc1file;
					etc1tool.Execute ();

					items.Add (new TaskItem (etc1file));
					items.Add (new TaskItem (alphafile));

					if (File.Exists (item.ItemSpec)) {
						try {
						File.Delete (item.ItemSpec);
						} catch(IOException ex) {
							// read only error??
							Log.LogErrorFromException (ex);
						}
					}

				} else {
					items.Add (item);
				}

			}
			OutputFiles = items.ToArray ();
			return !Log.HasLoggedErrors;
		}

		public class Etc1Tool {

			public string Source { get; set; }

			public string Destination { get; set; }

			public string AndroidSdkDir { get; set; }

			public void Execute() {

				var tool = Path.Combine (AndroidSdkDir, "tools/etc1tool");

				var process = new System.Diagnostics.Process ();
				process.StartInfo.FileName = tool;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				process.StartInfo.Arguments = string.Format (" {0} --encode -o {1}", Source, Destination);
				process.StartInfo.CreateNoWindow = true;
				process.Start ();
				process.WaitForExit ();
			}
		}
	}
}

