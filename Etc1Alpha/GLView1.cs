using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;
using OpenTK.Platform.Android;

using Android.Views;
using Android.Content;
using Android.Util;
using Android.App;
using System.Text;
#if __ANDROID__
using ShaderType = OpenTK.Graphics.ES20.All;
#endif

namespace Etc1Alpha
{
	class GLView1 : AndroidGameView
	{
		static float transY = 0.0f;
		const int UNIFORM_TRANSLATE = 0;
		const int UNIFORM_TEXTURE = 1;
		const int UNIFORM_COUNT = 2;
		readonly int[] uniforms = new int [UNIFORM_COUNT];
		const int ATTRIB_VERTEX = 0;
		const int ATTRIB_TEXTURECOORD = 1;
		const int ATTRIB_COUNT = 2;
		int program;
		int texture = 0;

		public GLView1 (Context context) : base (context)
		{
		}

		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

			GL.BlendFunc (All.SrcAlpha, All.OneMinusSrcAlpha);
			GL.Enable (All.Blend);

			LoadShaders ();

			texture = GLSupport.LoadTextureFromAssets ("f_spot_rgb.etc1");

			// Run the render loop
			Run ();
		}

		bool LoadShaders ()
		{
			int vertShader, fragShader;

			// Create shader program.
			program = GL.CreateProgram ();

			// Create and compile vertex shader.
			var vertShaderPathname = "Shader.vsh";
			if (!GLSupport.CompileShader (ShaderType.VertexShader, vertShaderPathname, out vertShader)) {
				System.Diagnostics.Debug.WriteLine ("Failed to compile vertex shader");
				return false;
			}

			// Create and compile fragment shader.
			var fragShaderPathname = "Shader.fsh";
			if (!GLSupport.CompileShader (ShaderType.FragmentShader, fragShaderPathname, out fragShader)) {
				System.Diagnostics.Debug.WriteLine ("Failed to compile fragment shader");
				return false;
			}

			// Attach vertex shader to program.
			GL.AttachShader (program, vertShader);

			// Attach fragment shader to program.
			GL.AttachShader (program, fragShader);

			// Bind attribute locations.
			// This needs to be done prior to linking.
			GL.BindAttribLocation (program, ATTRIB_VERTEX, "position");
			GL.BindAttribLocation (program, ATTRIB_TEXTURECOORD, "a_TexCoordinate");

			// Link program.
			if (!GLSupport.LinkProgram (program)) {
				System.Diagnostics.Debug.WriteLine ("Failed to link program: {0:x}", program);

				if (vertShader != 0)
					GL.DeleteShader (vertShader);

				if (fragShader != 0)
					GL.DeleteShader (fragShader);

				if (program != 0) {
					GL.DeleteProgram (program);
					program = 0;
				}

				return false;
			}

			// Get uniform locations.
			#if __IOS__
			uniforms [UNIFORM_TRANSLATE] = GL.GetUniformLocation (program, "translate");
			uniforms [UNIFORM_TEXTURE] = GL.GetUniformLocation (program, "u_Texture");
			#elif __ANDROID__
			uniforms [UNIFORM_TRANSLATE] = GL.GetUniformLocation (program, new StringBuilder ("translate"));
			uniforms [UNIFORM_TEXTURE] = GL.GetUniformLocation (program, new StringBuilder ("u_Texture"));
			#endif

			// Release vertex and fragment shaders.
			if (vertShader != 0) {
				GL.DetachShader (program, vertShader);
				GL.DeleteShader (vertShader);
			}

			if (fragShader != 0) {
				GL.DetachShader (program, fragShader);
				GL.DeleteShader (fragShader);
			}

			return true;
		}

		void DestroyShaders ()
		{
			if (program != 0) {
				GL.DeleteProgram (program);
				program = 0;
			}
		}

		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		//
		// In this particular case, we demonstrate how to set
		// the graphics mode and fallback in case the device doesn't
		// support the defaults
		protected override void CreateFrameBuffer ()
		{
			ContextRenderingApi = OpenTK.Graphics.GLVersion.ES2;
			GraphicsMode = new AndroidGraphicsMode (new ColorFormat (32), 24, 0, 0, 0, false);
			try {
				Log.Verbose ("GLCube", "Loading with default settings");

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLCube", "{0}", ex);
			}

			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try {
				Log.Verbose ("GLCube", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLCube", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}

		// This gets called on each frame render
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			// you only need to call this if you have delegates
			// registered that you want to have called
			base.OnRenderFrame (e);

			MakeCurrent ();

			// Replace the implementation of this method to do your own custom drawing.
			GL.ClearColor (0.5f, 0.5f, 0.5f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// Use shader program.
			GL.UseProgram (program);

			// Update uniform value.
			GL.Uniform1 (uniforms [UNIFORM_TRANSLATE], transY);
			transY += 0.0075f;

			// Update attribute values.
			GL.EnableVertexAttribArray (ATTRIB_VERTEX);
			GL.VertexAttribPointer (ATTRIB_VERTEX, 2, All.Float, false, 0, squareVertices);


			GL.EnableVertexAttribArray (ATTRIB_TEXTURECOORD);
			GL.VertexAttribPointer (ATTRIB_TEXTURECOORD, 2, All.Float, false, 0, squareTexCoords);


			GL.ActiveTexture (All.Texture0);
			GL.BindTexture (All.Texture2D, texture);

			GL.Uniform1 (uniforms [UNIFORM_TEXTURE], 0);

			GL.DrawArrays (All.TriangleStrip, 0, 4);

			SwapBuffers ();
		}

		static readonly float[] squareVertices = {
			-1f, -1f,
			1f, -1f,
			-1f,  1f,
			1f,  1f,
		};
		static readonly float[] squareTexCoords = {
			0.0f, 0.0f, 				
			0.0f, 1.0f,
			1.0f, 0.0f,
			1.0f, 1.0f,
		};


	}
}

