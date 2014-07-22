// Copyright(C) David W. Jeske, 2013
// Released to the public domain. Use, modify and relicense at will.

using System;
using System.IO;

using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SimpleScene
{
	// TODO: add delegate interface for providing the texture-surface (aka reloading)
	// TODO: add support for OpenGL texture eviction extension (i forget the name of the ext)

	public class SSTexture
	{	
		private int _glTextureID = 0;
		public int TextureID { get { return _glTextureID; } }
		public SSTexture () { }

        public void DeleteTexture() {
            GL.DeleteTexture(_glTextureID);
            _glTextureID = 0;
        }

		public void createFromBitmap(Bitmap TextureBitmap, string name=null, bool hasAlpha=false) {		    
		    //get the data out of the bitmap
            System.Drawing.Imaging.BitmapData TextureData;

            if (name == null) {
                name = Bend.WhoCalls.StackTrace();
            }

            if (hasAlpha) {
                TextureData = TextureBitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb
                    );

            } else {
                TextureData = TextureBitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb
                    );
            }
		 
		    //Code to get the data to the OpenGL Driver
		  
			GL.Enable (EnableCap.Texture2D);
			GL.ActiveTexture(TextureUnit.Texture0);

		    //generate one texture and put its ID number into the "_glTextureID" variable
		    GL.GenTextures(1,out _glTextureID);
		    //tell OpenGL that this is a 2D texture
		    GL.BindTexture(TextureTarget.Texture2D,_glTextureID);
		 
		    //the following code sets certian parameters for the texture
			// GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Combine);
			// GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (float)TextureEnvMode.Modulate);

			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
			// this assumes mipmaps are present...
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);

		    
		    // tell OpenGL to build mipmaps out of the bitmap data
			// .. what a mess ... http://www.g-truc.net/post-0256.html
			// this is the old way, must be called before texture is loaded, see below for new way...
		    // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (float)1.0f);
		 
			// tell openGL the next line begins on a word boundary...
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

		    // load the texture
            if (hasAlpha) {
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0, // level
                    PixelInternalFormat.CompressedRgba,
                    TextureBitmap.Width, TextureBitmap.Height,
                    0, // border
                    PixelFormat.Bgra,     // why is this Bgr when the lockbits is rgb!?
                    PixelType.UnsignedByte,
                    TextureData.Scan0
                    );
                GL.GetError();
                Console.WriteLine("SSTexture: loaded alpha ({0},{1}) from: {2}", TextureBitmap.Width, TextureBitmap.Height, name);
            } else {
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0, // level
                    PixelInternalFormat.CompressedRgb,
                    TextureBitmap.Width, TextureBitmap.Height,
                    0, // border
                    PixelFormat.Bgr,     // why is this Bgr when the lockbits is rgb!?
                    PixelType.UnsignedByte,
                    TextureData.Scan0
                    );
                GL.GetError();
                Console.WriteLine("SSTexture: loaded ({0},{1}) from: {2}", TextureBitmap.Width, TextureBitmap.Height, name);
            }

			// this is the new way to generate mipmaps
			GL.GenerateMipmap (GenerateMipmapTarget.Texture2D);

		    //free the bitmap data (we dont need it anymore because it has been passed to the OpenGL driver
		    TextureBitmap.UnlockBits(TextureData);		 
		}
		
	}
}
