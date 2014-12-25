﻿using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SimpleScene
{
    public class SSObjectBillboard : SSObjectMesh
    {
        public Vector3 color = new Vector3 (1f);

        public bool IsOcclusionQueueryEnabled = false;

        private int m_queuery;

        public int QueueryResult {
            get {
                int ret;
                GL.GetQueryObject(m_queuery, GetQueryObjectParam.QueryResult, out ret);
                return ret;
            }
        }

        public SSObjectBillboard ()
        {
            m_queuery = GL.GenQuery();
        }

        public SSObjectBillboard(SSAbstractMesh mesh)
            : this()
        {
            Mesh = mesh;
        }

        public override void Render(ref SSRenderConfig renderConfig)
        {
            if (Mesh != null) {
                base.Render(ref renderConfig);

                #if true
                // override matrix setup to get rid of any rotation in view
                // http://stackoverflow.com/questions/5467007/inverting-rotation-in-3d-to-make-an-object-always-face-the-camera/5487981#5487981
                Matrix4 modelViewMat = this.worldMat * renderConfig.invCameraViewMat;
                Vector3 trans = modelViewMat.ExtractTranslation();
                //Vector3 scale = modelViewMat.ExtractScale();
                modelViewMat = new Matrix4 (
                    Scale.X, 0f, 0f, trans.X,
                    0f, Scale.Y, 0f, trans.Y,
                    0f, 0f, Scale.Z, trans.Z,
                    0f, 0f, 0f, 1f);
                modelViewMat.Transpose();
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadMatrix(ref modelViewMat);
                #endif

                GL.Color3(color);

                if (IsOcclusionQueueryEnabled) {
                    GL.BeginQuery(QueryTarget.SamplesPassed, m_queuery);
                }

                Mesh.RenderMesh(ref renderConfig);

                if (IsOcclusionQueueryEnabled) {
                    GL.EndQuery(QueryTarget.SamplesPassed);
                }
            }
        }
    }
}
