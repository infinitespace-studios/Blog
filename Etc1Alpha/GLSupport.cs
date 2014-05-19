using System;
using Android.App;
using OpenTK.Graphics.ES20;
#if __ANDROID__
using ShaderParameter = OpenTK.Graphics.ES20.All;
using ShaderType = OpenTK.Graphics.ES20.All;
using ProgramParameter = OpenTK.Graphics.ES20.All;
#endif

namespace Etc1Alpha
{
	public static class GLSupport
	{
		#if __ANDROID__
		public static Activity Activity {get;set;}

		public static int LoadTextureFromAssets (string filename)
		{

			using (var s = Activity.Assets.Open (filename)) {
				using (var t = Android.Opengl.ETC1Util.CreateTexture (s)) {

					int tid = GL.GenTexture ();
					GL.ActiveTexture (All.Texture0);
					GL.BindTexture (All.Texture2D, tid);
					// setup texture parameters
					GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
					GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
					GL.TexParameter (All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
					GL.TexParameter (All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
					Android.Opengl.ETC1Util.LoadTexture ((int)All.Texture2D, 0, 0, (int)All.Rgb, (int)All.UnsignedShort565, t);

					return tid;
				}
			}
		}
		#endif
			
		#region Shader utilities

		static string ShaderSource (string filename)
		{
#if __ANDROID__
			return new System.IO.StreamReader (Activity.Assets.Open (String.Format ("{0}", filename))).ReadToEnd ();
#elif __IOS__
			var f = Path.GetFileNameWithoutExtension (filename);
			var ext = Path.GetExtension (filename);
			var p = NSBundle.MainBundle.PathForResource (f, ext.Replace(".",""));
			return System.IO.File.ReadAllText (p);
#endif
		}
			
		public static bool CompileShader (ShaderType type, string file, out int shader)
		{
			string src = ShaderSource (file);

			shader = GL.CreateShader (type);
			GL.ShaderSource (shader, 1, new string[] { src }, (int[])null);
			GL.CompileShader (shader);

			#if DEBUG
			int logLength;
			GL.GetShader (shader, ShaderParameter.InfoLogLength, out logLength);
			if (logLength > 0) {
				var data = GL.GetShaderInfoLog (shader);
				System.Diagnostics.Debug.WriteLine ("Shader compile log:\n{0}", data);
			}
			#endif
			int status;
			GL.GetShader (shader, ShaderParameter.CompileStatus, out status);
			if (status == 0) {
				GL.DeleteShader (shader);
				return false;
			}

			return true;
		}

		public static bool LinkProgram (int program)
		{
			GL.LinkProgram (program);

			#if DEBUG
			int logLength;
			GL.GetProgram (program, ProgramParameter.InfoLogLength, out logLength);
			if (logLength > 0) {
				var data = GL.GetProgramInfoLog (program);
				System.Diagnostics.Debug.WriteLine ("Program link log:\n{0}", data);
			}
			#endif
			int status;
			GL.GetProgram (program, ProgramParameter.LinkStatus, out status);
			if (status == 0)
				return false;

			return true;
		}

		#endregion
	}
}

