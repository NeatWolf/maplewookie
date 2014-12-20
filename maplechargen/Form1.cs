﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace maplechargen {
	public partial class Form1 : Form {
		public Bitmap bmp;

		public Form1() {
			InitializeComponent();

			bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
			Generate();
		}

		private void onGenerateClicked(object sender, EventArgs e) {
			Generate();
		}

		public void Generate() {
			Graphics g = Graphics.FromImage(bmp);
			g.Clear(Color.Transparent);

			int xo = 100;
			int yo = 100;

			for (int i = 0; i < 1000; i++) {
				g.Clear(Color.Transparent);
				Character c = Character.random(0);
				c.render(g, xo, yo);
				TrimBitmap(bmp).Save(@"C:\output\" + i + ".png");
			}

			pictureBox1.Image = bmp;
		}

		// google had this in store for me
		// god invented copy & paste for a reason
		static Bitmap TrimBitmap(Bitmap source) {
			Rectangle srcRect = default(Rectangle);
			BitmapData data = null;
			try {
				data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				byte[] buffer = new byte[data.Height * data.Stride];
				Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

				int xMin = int.MaxValue,
					xMax = int.MinValue,
					yMin = int.MaxValue,
					yMax = int.MinValue;

				bool foundPixel = false;

				// Find xMin
				for (int x = 0; x < data.Width; x++) {
					bool stop = false;
					for (int y = 0; y < data.Height; y++) {
						byte alpha = buffer[y * data.Stride + 4 * x + 3];
						if (alpha != 0) {
							xMin = x;
							stop = true;
							foundPixel = true;
							break;
						}
					}
					if (stop)
						break;
				}

				// Image is empty...
				if (!foundPixel)
					return null;

				// Find yMin
				for (int y = 0; y < data.Height; y++) {
					bool stop = false;
					for (int x = xMin; x < data.Width; x++) {
						byte alpha = buffer[y * data.Stride + 4 * x + 3];
						if (alpha != 0) {
							yMin = y;
							stop = true;
							break;
						}
					}
					if (stop)
						break;
				}

				// Find xMax
				for (int x = data.Width - 1; x >= xMin; x--) {
					bool stop = false;
					for (int y = yMin; y < data.Height; y++) {
						byte alpha = buffer[y * data.Stride + 4 * x + 3];
						if (alpha != 0) {
							xMax = x;
							stop = true;
							break;
						}
					}
					if (stop)
						break;
				}

				// Find yMax
				for (int y = data.Height - 1; y >= yMin; y--) {
					bool stop = false;
					for (int x = xMin; x <= xMax; x++) {
						byte alpha = buffer[y * data.Stride + 4 * x + 3];
						if (alpha != 0) {
							yMax = y;
							stop = true;
							break;
						}
					}
					if (stop)
						break;
				}

				srcRect = Rectangle.FromLTRB(xMin, yMin, xMax + 1, yMax + 1);
			} finally {
				if (data != null)
					source.UnlockBits(data);
			}

			Bitmap dest = new Bitmap(srcRect.Width, srcRect.Height);
			Rectangle destRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
			using (Graphics graphics = Graphics.FromImage(dest)) {
				graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel);
			}
			return dest;
		}
	}
}