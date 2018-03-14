using UnityEngine;
using MetriCam2.Cameras;
using Metrilus.Util;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;


/**
 * -------------------------------------------------------------------------
 *  빌드 후 중요사항
 *  빌드를 한 후에 program이름_Data 폴더 내부에 OpenNi2 폴더를 넣어야함
 *  넣지않으면 빌드된 프로그램 실행시 camera.Connect()가 실행되지 않음
 *  
 *  After built : OpenNi2 folder should be in ther (programName)_Data.
 *  If you not, camera.Connect() will not be run.
 * -------------------------------------------------------------------------
*/
public class Xtion2Script : MonoBehaviour
{

    private MetriCam2.Camera camera;
    System.Drawing.Bitmap img_color;
    public RawImage ColorImg;
    public RawImage DepthImg;

    Gradient BlackToWhite;

    private bool f = false;

    // Use this for initialization
    void Start()
    {
        camera = new Xtion2();

        camera.Connect();

        BlackToWhite = new Gradient();

        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        gck = new GradientColorKey[8];
        gck[0].color = Color.black;
        gck[0].time = 0.0F;
        gck[1].color = Color.red;
        gck[1].time = 1/8f;
        gck[2].color = Color.blue;
        gck[2].time = 2/8F;
        gck[3].color = Color.gray;
        gck[3].time = 3/8f;
        gck[4].color = Color.green;
        gck[4].time = 4/8F;
        gck[5].color = Color.yellow;
        gck[5].time = 5/8f;
        gck[6].color = Color.cyan;
        gck[6].time = 6/8F;
        gck[7].color = Color.white;
        gck[7].time = 7/8f;



        gak = new GradientAlphaKey[8];
        gak[0].alpha = 1.0f;
        gak[0].time = 0f;
        gak[1].alpha = 1.0f;
        gak[1].time = 1.0f;
        gak[2].alpha = 1.0f;
        gak[2].time = 1.0f;
        gak[3].alpha = 1.0f;
        gak[3].time = 1.0f;
        gak[4].alpha = 1.0f;
        gak[4].time = 1.0f;
        gak[5].alpha = 1.0f;
        gak[5].time = 1.0f;
        gak[6].alpha = 1.0f;
        gak[6].time = 1.0f;
        gak[7].alpha = 1.0f;
        gak[7].time = 1.0f;
        
        BlackToWhite.SetKeys(gck, gak);
    }

    bool once = false;
    // Update is called once per frame
    void Update()
    {

        ColorImage();


        FloatImage();

    }

    private void OnDestroy()
    {
        camera.Disconnect();
    }

    public void ColorImage()
    {
        if (camera.IsConnected)
        {
            camera.Update();
            ColorCameraImage img = (ColorCameraImage)camera.CalcChannel(MetriCam2.ChannelNames.Color);
            img_color = img.ToBitmap();
            System.Drawing.Bitmap b = new System.Drawing.Bitmap(img_color);
            Texture2D tx = new Texture2D(img_color.Width, img_color.Height, TextureFormat.BGRA32, false);
            tx.LoadRawTextureData(imageToByteArray(b));
            tx.Apply();
            ColorImg.texture = tx;

            img.Dispose();
        }
    }

    unsafe public void FloatImage()
    {
        float min = 1;
        float max = 0;
        if (camera.IsConnected)
        {
            camera.Update();
            FloatCameraImage img = (FloatCameraImage)camera.CalcChannel(MetriCam2.ChannelNames.ZImage);
            float* img_float = img.Data;

            for (int i = 0; i < img.Width * img.Height; i++)
            {
                if (img_float[i] > max)
                {
                    max = img_float[i];
                }
                else if (img_float[i] < min)
                {
                    min = img_float[i];
                }
            }
            

            Texture2D tex = new Texture2D(img.Width, img.Height);

            Color[] depthColor = new Color[img.Width * img.Height];

            float temp = max - min;

            int fool = 30;
            int tmp = 5;

            for (int i = 0; i < img.Width * img.Height; i++)
            {
                if (img_float[i] > 800 && img_float[i] < 1200)
                {
                    depthColor[i] = BlackToWhite.Evaluate((img_float[i] - 800) / 400);
                }
                else
                {
                    depthColor[i] = Color.black;
                }

                //if (img_float[i] % 100 <50)
                //{
                //    depthColor[i] = Color.black;
                //}
                //else
                //{
                //    depthColor[i] = Color.white;
                //}

                //depthColor[i] = BlackToWhite.Evaluate(img_float[i] / temp);
            }

            tex.SetPixels(depthColor);
            tex.Apply();
            DepthImg.texture = tex;

            img.Dispose();
        }
    }

    public byte[] imageToByteArray(System.Drawing.Image imageIn)
    {
        int t1 = Environment.TickCount;
        var o = System.Drawing.GraphicsUnit.Pixel;
        System.Drawing.RectangleF r1 = imageIn.GetBounds(ref o);
        System.Drawing.Rectangle r2 = new System.Drawing.Rectangle((int)r1.X, (int)r1.Y, (int)r1.Width, (int)r1.Height);
        System.Drawing.Imaging.BitmapData omg = ((System.Drawing.Bitmap)imageIn).LockBits(r2, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        byte[] rgbValues = new byte[r2.Width * r2.Height * 4];
        Marshal.Copy((IntPtr)omg.Scan0, rgbValues, 0, rgbValues.Length);
        ((System.Drawing.Bitmap)imageIn).UnlockBits(omg);

        return rgbValues;
    }

}
