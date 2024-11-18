using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using Windows.UI; // Color�� ���� ���ӽ����̽� �߰�
using System.IO;
using System.Text.Json;
using Windows.Storage.Pickers; // FileSavePicker�� ���� ���ӽ����̽� �߰�
using Windows.Storage; // StorageFile�� ���� ���ӽ����̽� �߰�
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI;
using System;

namespace App16
{
    public sealed partial class MainWindow : Window
    {
        private bool flag;
        private float px, py;
        private float mySize;
        private List<float> vx = new List<float>();
        private List<float> vy = new List<float>();
        private List<Color> col = new List<Color>();
        private List<float> size = new List<float>();
        private Color myCol;

        public MainWindow()
        {
            this.InitializeComponent();
            flag = false;
            px = 100;
            py = 100;
            mySize = 16;
            myCol = Colors.Green; // �ʱ� ���� ����
        }

        // �׸��� �Լ�
        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            int n = vx.Count;
            for (int i = 1; i < n; i++)
            {
                if (vx[i] == 0 && vy[i] == 0)
                {
                    i++;
                    continue;
                }
                args.DrawingSession.DrawLine(vx[i - 1], vy[i - 1], vx[i], vy[i], col[i], size[i]);
                args.DrawingSession.FillCircle(vx[i - 1], vy[i - 1], size[i] / 2, col[i]);
                args.DrawingSession.FillCircle(vx[i], vy[i], size[i] / 2, col[i]);
            }
        }

        // ���콺 Ŭ�� �� �׸��� ����
        private void CanvasControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            flag = true;
        }

        // ���콺 �̵� �� �׸���
        private void CanvasControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var canvasControl = sender as CanvasControl; // ĵ���� ��Ʈ�� ����
            var point = e.GetCurrentPoint(canvasControl);
            px = (float)point.Position.X;
            py = (float)point.Position.Y;

            if (flag)
            {
                vx.Add(px);
                vy.Add(py);
                col.Add(myCol);
                size.Add(mySize);
                canvas.Invalidate(); // ĵ������ �ٽ� �׸��� ���� ��ȿȭ
            }
        }

        // ���콺 Ŭ�� ���� �� �׸��� ����
        private void CanvasControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            flag = false;
            px = py = 0.0f;
            vx.Add(px);
            vy.Add(py);
            col.Add(myCol);
            size.Add(mySize);
        }

        // ���� ���ñⰡ ����Ǹ� ���� ������Ʈ
        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            myCol = args.NewColor;
        }

        // �����̴� ���� ���� �귯�� ũ�� ������Ʈ
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            mySize = (float)e.NewValue;
        }

        // ȭ�� ����� ��ư Ŭ�� �� ����
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            vx.Clear();
            vy.Clear();
            col.Clear();
            size.Clear();

            canvas.Invalidate(); // ȭ���� �ٽ� �׷��� ���� (FindName ��� ���� canvas ���)
        }

        // ���� ��ư Ŭ�� �� ���� - FileSavePicker�� ����Ͽ� ����� ���� ���ϸ����� ����
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker();

            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            picker.FileTypeChoices.Add("JSON Files", new List<string>() { ".json" });

            picker.SuggestedFileName = "drawingData";

            StorageFile file = await picker.PickSaveFileAsync();

            if (file != null)
            {
                var drawingData = new DrawingData()
                {
                    Vx = vx,
                    Vy = vy,
                    Col = col,
                    Size = size
                };

                string jsonString = JsonSerializer.Serialize(drawingData);

                await FileIO.WriteTextAsync(file, jsonString); // ������ ���ϸ����� ������ ����
            }
        }

        // �ҷ����� ��ư Ŭ�� �� ����
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("drawingData.json"))
            {
                string jsonString = File.ReadAllText("drawingData.json");

                var drawingData = JsonSerializer.Deserialize<DrawingData>(jsonString);

                vx = drawingData.Vx ?? new List<float>();
                vy = drawingData.Vy ?? new List<float>();
                col = drawingData.Col ?? new List<Color>();
                size = drawingData.Size ?? new List<float>();

                canvas.Invalidate(); // �����͸� �ҷ��� �� ĵ������ �ٽ� �׸� (FindName ��� ���� canvas ���)
            }
        }
    }

    // �׸��� �����͸� ������ Ŭ���� ����
    public class DrawingData
    {
        public List<float> Vx { get; set; }
        public List<float> Vy { get; set; }
        public List<Color> Col { get; set; }
        public List<float> Size { get; set; }
    }
}