using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows.Forms;

namespace extracteurmasque
{

    public class FormColorSelector : Form
    {
        Bitmap bmpImage;

        public Color RefColor { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

        public FormColorSelector(string path) {
            //Application.DoEvents();
            bmpImage = new Bitmap(path);
            ClientSize = new Size(bmpImage.Width, bmpImage.Height);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.DrawImage(bmpImage, 0, 0);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var pos = e.Location;
            PosX = pos.X;
            PosY = pos.Y;
            RefColor = bmpImage.GetPixel(pos.X, pos.Y);
        }

    }

    public class FormColorPaint : Form
    {
        static Bitmap bmpImage;
        static string imgPath;
        static Bitmap mask;
        static Graphics g;
        Bitmap overlayImage;

        public Color RefColor { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

        public FormColorPaint(string path)
        {
            //Application.DoEvents();
            bmpImage = new Bitmap(path);
            imgPath = path;
            ClientSize = new Size(bmpImage.Width, bmpImage.Height);
            mask = new Bitmap(bmpImage.Width, bmpImage.Height);
            g = Graphics.FromImage(mask);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, bmpImage.Width, bmpImage.Height);
            overlayImage = new Bitmap(bmpImage.Width, bmpImage.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.DrawImage(bmpImage, 0, 0);
            g.DrawImage(overlayImage, Point.Empty);
            
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var pos = e.Location;
            PosX = pos.X;
            PosY = pos.Y;
            RefColor = bmpImage.GetPixel(pos.X, pos.Y);
            PaintByClick(RefColor, PosX, PosY);

            Refresh();
        }

        

        double ColorDist(Color c1, Color c2)
        {
            return Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B));
        }

        void PaintByClick(Color refColor, int posX, int posY)
        {
            int regionHalfLength = 50;
            //MessageBox.Show(refColor.ToString() + "; x = " + posX.ToString() + ", y = " + posY.ToString());
            //var orig = new Bitmap(path);
            int origX;
            origX = (posX > regionHalfLength) ? posX - regionHalfLength : 0;
            int origY;
            origY = (posY > regionHalfLength) ? posY - regionHalfLength : 0;

            int topX;
            topX = (posX < (bmpImage.Width - regionHalfLength)) ? posX + regionHalfLength : bmpImage.Width;
            int topY;
            topY = (posY < (bmpImage.Height - regionHalfLength)) ? posY + regionHalfLength : bmpImage.Height;

            //MessageBox.Show(refColor.ToString() + "; x = " + posX.ToString() + ", y = " + posY.ToString() + ", origX = " + origX.ToString() + ", origY = " + origY.ToString() + ", topX = " + topX.ToString() + ", topY = " + topY.ToString());
            //var origX = PosX - 50;
            //var origY = PosY - 50;

            //MessageBox.Show("origX = " + origX.ToString() + ", origY = " + origY.ToString() + ", topX = " + topX.ToString() + ", topY = " + topY.ToString());

            //var mask = new Bitmap(bmpImage.Width, bmpImage.Height);
            //Graphics g = Graphics.FromImage(mask);
            //g.FillRectangle(new SolidBrush(Color.Black), 0, 0, bmpImage.Width, bmpImage.Height);

            var brushSize = 4;

            for (int y = origY + (brushSize / 2); y < topY - brushSize / 2; y++)
            {
                for (int x = origX + (brushSize / 2); x < topX - brushSize / 2; x++)
                {
                    var currPixel = bmpImage.GetPixel(x, y);
                    var dist = ColorDist(currPixel, refColor);
                    if (dist > 8)
                    {
                        g.FillRectangle(new SolidBrush(Color.Black), x, y, 1, 1);
                    }
                    else
                    {
                        g.FillEllipse(new SolidBrush(Color.White), x - brushSize / 2, y - brushSize / 2, brushSize, brushSize);
                        overlayImage.SetPixel(x, y, Color.Red);
                    }

                }
            }

            var ext = Path.GetExtension(imgPath);
            var savePath = Path.GetFileNameWithoutExtension(imgPath) + "_premask" + ext;

            savePath = Path.GetDirectoryName(imgPath) + "\\" + savePath;
            mask.Save(savePath);
        }
    }

    public partial class Form1 : Form
    {
        PictureBox pictureBox = new PictureBox();


        string? path;


        public Form1()
        {
            InitializeComponent();
            
            var btnOpenSingleFile = new Button();
            btnOpenSingleFile.Text = "Ouvrir";
            this.Controls.Add(btnOpenSingleFile);
            btnOpenSingleFile.Location = new Point(0, 20);
            btnOpenSingleFile.Click += B_Click;
            btnOpenSingleFile.Width = 110;


            pictureBox.Location = new Point(50, 150);
            pictureBox.Size = new Size(200, 200);
            this.Controls.Add(pictureBox);
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            var btnSaveMask = new Button();
            btnSaveMask.Text = "Sauvegarder Masque";
            btnSaveMask.Click += SaveMask_Click;
            btnSaveMask.Location = new Point(150, 20);
            btnSaveMask.Width = 110;
            this.Controls.Add(btnSaveMask);

            var btnBatch = new Button();
            btnBatch.Text = "Ouvrir Dossier";
            btnBatch.Width = 110;
            this.Controls.Add(btnBatch);
            btnBatch.Location = new Point(300 , 20);
            btnBatch.Click += BtnBatch_Click;

            var btnBatchPrep = new Button();
            btnBatchPrep.Text = "Preproc";
            btnBatchPrep.Width = 110;
            this.Controls.Add(btnBatchPrep);
            btnBatchPrep.Location = new Point(450, 20);
            btnBatchPrep.Click += BtnBatchPrep_Click;

            var btnPaintClick = new Button();
            btnPaintClick.Text = "Méthode 2";
            btnPaintClick.Width = 110;
            this.Controls.Add(btnPaintClick);
            btnPaintClick.Location = new Point(600, 20);
            btnPaintClick.Click += BtnPaintClick_Click;
        }

        double ColorDist(Color c1, Color c2)
        {
            return Math.Sqrt( (c1.R - c2.R) * (c1.R - c2.R) + (c1.G - c2.G) * (c1.G - c2.G) + (c1.B - c2.B) * (c1.B - c2.B));
        }

        void PreProcessing(string path, Color refColor)
        {
            var orig = new Bitmap(path);
            var mask = new Bitmap(orig.Width, orig.Height);
            Graphics g = Graphics.FromImage(mask);


            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, mask.Width, mask.Height);

            var brushSize = 4;

            for (int y = brushSize / 2; y < orig.Height - brushSize / 2; y++)
            {
                for (int x = brushSize / 2; x < orig.Width - brushSize / 2; x++)
                {
                    var currPixel = orig.GetPixel(x, y);
                    var dist = ColorDist(currPixel, refColor);
                    if (dist > 10)
                    {
                        g.FillRectangle(new SolidBrush(Color.Black), x, y, 1, 1);
                    }else
                    {
                        g.FillEllipse(new SolidBrush(Color.White), x - brushSize / 2, y - brushSize / 2, brushSize, brushSize);
                    }

                }
            }

            var ext = Path.GetExtension(path);
            var savePath = Path.GetFileNameWithoutExtension(path) + "_premask" + ext;

            savePath = Path.GetDirectoryName(path) + "\\" + savePath;
            mask.Save(savePath);
        }


        private void BtnBatchPrep_Click(object? sender, EventArgs e)
        {
            var openFld = new FolderBrowserDialog();
            if (openFld.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in Directory.GetFiles(openFld.SelectedPath))
                {
                    var frmSel = new FormColorSelector(file);
                    if (frmSel.ShowDialog() != DialogResult.OK)
                    {
                        //MessageBox.Show(frmSel.RefColor.ToString());
                        PreProcessing(file, /*Color.FromArgb(214, 180, 178)*/frmSel.RefColor);
                    }

                }
                MessageBox.Show("OK");
            }
        }

        private void BtnPaintClick_Click(object? sender, EventArgs e) 
        {
            var openFld = new FolderBrowserDialog();
            if (openFld.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in Directory.GetFiles(openFld.SelectedPath))
                {
                    var frmPaint = new FormColorPaint(file);
                    if (frmPaint.ShowDialog() != DialogResult.OK)
                    {
                        //MessageBox.Show(frmPaint.PosX.ToString() + ", " + frmPaint.PosY.ToString());
                        //PaintByClick(file, frmPaint.RefColor, frmPaint.PosX, frmPaint.PosY);
                    }

                }
                MessageBox.Show("OK");
            }

        }

        private void BtnBatch_Click(object? sender, EventArgs e)
        {
            var openFld = new FolderBrowserDialog();
            if (openFld.ShowDialog() == DialogResult.OK )
            {
                foreach(var file in Directory.GetFiles(openFld.SelectedPath))
                {
                    ExtractMask(file);
                }
                MessageBox.Show("OK");
            }
        }


        void ExtractMask(string path)
        {
            var orig = new Bitmap(path);
            var mask = new Bitmap(orig.Width, orig.Height);
            

            for (int y = 0; y < orig.Height; y++)
            {
                for (int x = 0; x < orig.Width; x++)
                {
                    if (orig.GetPixel(x, y).G == 255 && orig.GetPixel(x, y).R == 0 && orig.GetPixel(x, y).B == 0)
                    {
                        mask.SetPixel(x, y, Color.White);
                    }
                    else
                        mask.SetPixel(x, y, Color.Black);
                }
            }

            var ext = Path.GetExtension(path);
            var savePath = Path.GetFileNameWithoutExtension(path) + "_mask" + ext;

            savePath = Path.GetDirectoryName(path) + "\\" +savePath;
            mask.Save(savePath);
        }

        private void SaveMask_Click(object? sender, EventArgs e)
        {
            var sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.OK)
            {
                if (path != null)
                    ExtractMask(path);
            }
        }

        private void B_Click(object? sender, EventArgs e)
        {
            var of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                path = of.FileName;
                pictureBox.Image = Bitmap.FromFile(path);
                pictureBox.Refresh();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
