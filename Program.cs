using OpenCvSharp;
using Tesseract;
using Rect = OpenCvSharp.Rect;

Mat immagine = Cv2.ImRead(@"C:\Users\almar\Desktop\visual studio\ProvaCvC\foto\prova.jpg", ImreadModes.Color);   //Path per caricare l'immagine
Cv2.Resize(immagine, immagine, new Size(), 3, 3); // Ingradisco l'immagine
Mat p = immagine.Clone(); // Clono l'mmagine


Cv2.CvtColor(immagine, immagine, ColorConversionCodes.BGR2HSV); //  Cambio il colore della foto da BRG a HSV Per rendere facile il riconosmento della targa dai rettangolo blu 
Cv2.InRange(immagine, new Scalar(100, 150, 50), new Scalar(140, 255, 255), immagine); // Soglia per riconoscere il blu e covertiro in biango e in nero 
Cv2.MorphologyEx(immagine, immagine, MorphTypes.Close, Cv2.GetStructuringElement(MorphShapes.Rect, new Size(11, 11)));  // dilato(espande il bianco) poi erodo(erode il bianco)
Cv2.FindContours(immagine, out var Punti, out var hi, RetrievalModes.List, ContourApproximationModes.ApproxSimple); // trovo i contorni delle regioni bianche

Rect RoiSinstra = new Rect(); // Faccio una Roi per il rettangolo sinstro 
Rect RoiDestra = new Rect(); // Faccio una Roi per il rettangolo destro 
foreach (var item in Punti)
{
    Rect k = Cv2.BoundingRect(item);  // Trovo i Punti
    int newWidth = (int)(k.Width * 0.30);   //Faccio la largezza del rettangolo piu piccola del 60%
    int offsetX = (k.Width - newWidth) / 2; // Calcolo il nuovo Centro 
    k = new Rect(k.X + offsetX, k.Y, newWidth, k.Height);

    if (k.Width > 10 && k.Width <= 400 && k.Height > 200 && k.Height <= 1100) // Condizione per trovare i rettangolo
    {
        if (k.X + k.Width / 2 < p.Width / 2) // Condzioni per capire se e un rettangolo di sinistra o di destra
        {
            RoiSinstra = k; //Salvo il valore
            Cv2.Rectangle(p, RoiSinstra, Scalar.Blue, 2);
        }
        else
        {
            RoiDestra = k;  //Salvo il valore
            Cv2.Rectangle(p, RoiDestra, Scalar.Blue, 2);
        }
    }
}



Rect roiTT = new Rect(RoiSinstra.Width + RoiSinstra.X+(RoiSinstra.Width / 2), Math.Min(RoiSinstra.Y, RoiDestra.Y), RoiDestra.X - (RoiSinstra.Width + RoiSinstra.X + (RoiSinstra.Width / 2))-( RoiSinstra.Width/2), Math.Max(RoiSinstra.Height, RoiDestra.Height)+10); // Calcolo il rettangolo in mezzo hai rettangolo blu  
Mat f = new Mat(p, roiTT); // Taglio la foto con la roi(RoiTT)


Point Ss = new Point(RoiSinstra.X - roiTT.X, RoiSinstra.Y - roiTT.Y); // Calcolo il centro del rettangolo sinistro
Point Dd = new Point(RoiDestra.X - roiTT.X, RoiDestra.Y - roiTT.Y); //  Calcolo il centro del rettangolo destro 
double dx =( Dd.X + RoiDestra.Width / 2) -( Ss.X + RoiSinstra.Width / 2); // trovo le coordinate x 
double dy = (Dd.Y + RoiDestra.Height / 2) - (Ss.Y + RoiSinstra.Height / 2); // trovo le coordinate y
Point2f Centro = new Point2f(f.Width / 2, f.Height / 2); // trovo le coordinate del centro della roi(roiTT) 

double Angolo = Math.Atan2(dy, dx) * 180 / Math.PI; // Calcolo l'angolo per l'inglinazione 
Mat rotazione = Cv2.GetRotationMatrix2D(Centro, Angolo, 1); // Calocolo la matrice 2d per la rotazione 
Cv2.WarpAffine(f, f, rotazione, f.Size()); // Applico la rotazione


Cv2.CvtColor(f, f, ColorConversionCodes.BGR2GRAY);
Cv2.Threshold(f, f,75, 255, ThresholdTypes.Binary); // Applico una solgia per togliere rumore
Cv2.MorphologyEx(f, f, MorphTypes.Open, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(10, 10)));  // dilato(espande il bianco)

Cv2.ImWrite(@"C:\Users\almar\Desktop\visual studio\ProvaCvC\foto\targa.png", f); // Salvo l'immagine appena modificata

using (var engine = new TesseractEngine(@"C:\Users\almar\Desktop\visual studio\ProvaCvC\tessdata", "ita+eng", EngineMode.Default))  // uso Tesserac un OCR per conosere quello che che scrtto sull'immagne
{
    var fi = Pix.LoadFromFile(@"C:\Users\almar\Desktop\visual studio\ProvaCvC\foto\targa.png"); // Carico l'immagine

    using (var pi = engine.Process(fi, PageSegMode.SingleLine)) // Qua riconosco il testo
    {
        var testo = pi.GetText();
        var Precizione = pi.GetMeanConfidence();
        Console.WriteLine($"Testo estratto: {testo}");
        Console.WriteLine($"Preciso al {Precizione * 100}");
    }

}
