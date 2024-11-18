using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using Windows.UI; // Color를 위한 네임스페이스 추가
using System.IO;
using System.Text.Json;
using Windows.Storage.Pickers; // FileSavePicker를 위한 네임스페이스 추가
using Windows.Storage; // StorageFile을 위한 네임스페이스 추가
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
            myCol = Colors.Green; // 초기 색상 설정
        }

        // 그리기 함수
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

        // 마우스 클릭 시 그리기 시작
        private void CanvasControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            flag = true;
        }

        // 마우스 이동 시 그리기
        private void CanvasControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var canvasControl = sender as CanvasControl; // 캔버스 컨트롤 참조
            var point = e.GetCurrentPoint(canvasControl);
            px = (float)point.Position.X;
            py = (float)point.Position.Y;

            if (flag)
            {
                vx.Add(px);
                vy.Add(py);
                col.Add(myCol);
                size.Add(mySize);
                canvas.Invalidate(); // 캔버스를 다시 그리기 위해 무효화
            }
        }

        // 마우스 클릭 해제 시 그리기 중지
        private void CanvasControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            flag = false;
            px = py = 0.0f;
            vx.Add(px);
            vy.Add(py);
            col.Add(myCol);
            size.Add(mySize);
        }

        // 색상 선택기가 변경되면 색상 업데이트
        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            myCol = args.NewColor;
        }

        // 슬라이더 값에 따라 브러시 크기 업데이트
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            mySize = (float)e.NewValue;
        }

        // 화면 지우기 버튼 클릭 시 실행
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            vx.Clear();
            vy.Clear();
            col.Clear();
            size.Clear();

            canvas.Invalidate(); // 화면을 다시 그려서 지움 (FindName 대신 직접 canvas 사용)
        }

        // 저장 버튼 클릭 시 실행 - FileSavePicker를 사용하여 사용자 지정 파일명으로 저장
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

                await FileIO.WriteTextAsync(file, jsonString); // 선택한 파일명으로 데이터 저장
            }
        }

        // 불러오기 버튼 클릭 시 실행
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

                canvas.Invalidate(); // 데이터를 불러온 후 캔버스를 다시 그림 (FindName 대신 직접 canvas 사용)
            }
        }
    }

    // 그리기 데이터를 저장할 클래스 정의
    public class DrawingData
    {
        public List<float> Vx { get; set; }
        public List<float> Vy { get; set; }
        public List<Color> Col { get; set; }
        public List<float> Size { get; set; }
    }
}