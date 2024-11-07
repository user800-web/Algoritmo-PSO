using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSO_original_CSharp
{
    public partial class Form1 : Form
    {
        private const int n_particles = 30;
        private const int n_iterations = 10;
        private const double W = 0.5;
        private const double c1 = 0.8;
        private const double c2 = 0.9;
        private const double target_error = 1e-6;
        private List<Particle> particles = new List<Particle>();
        private double gBestValue = double.MaxValue;
        private PointF gBestPosition;
        //para Zoom del pictureBox
        private float zoomFactor = 1.0f; // Factor de zoom
        private PointF offset = new PointF(0, 0); // Desplazamiento

        public Form1()
        {
            InitializeComponent();
            InitializeParticles();
            // Asignar eventos al PictureBox
            this.pictureBox1.Paint += pictureBox1_Paint;
            this.pictureBox1.MouseWheel += pictureBox1_MouseWheel;
            this.pictureBox1.MouseDown += pictureBox1_MouseDown;
            this.pictureBox1.MouseMove += pictureBox1_MouseMove;
            this.pictureBox1.MouseUp += pictureBox1_MouseUp;
        }

        private void InitializeParticles()
        {
            Random rnd = new Random();
            for (int i = 0; i < n_particles; i++)
            {
                particles.Add(new Particle(rnd));
            }
        }

        // Función de aptitud (fitness)
        private double Fitness(PointF position)
        {
            double x = position.X;
            double y = position.Y;
            return x * x + y * y + 1;
        }

        // Actualiza el mejor valor personal (pBest) y global (gBest)
        private void UpdateBestValues()
        {
            foreach (var particle in particles)
            {
                double fitnessValue = Fitness(particle.Position);
                // Compara el mejor fitnessValue de la particula
                if (fitnessValue < particle.pBestValue)
                {
                    particle.pBestValue = fitnessValue;
                    particle.pBestPosition = particle.Position;
                }
                // Actualiza el mejor valor global si el fitness actual es menor
                if (fitnessValue < gBestValue)
                {
                    gBestValue = fitnessValue;
                    gBestPosition = particle.Position;
                }
            }
        }

        // Actualiza la velocidad y posición de las partículas
        private void UpdateParticles()
        {
            Random rnd = new Random();
            foreach (var particle in particles)
            {
                float inertiaX = (float)(W * particle.Velocity.X);
                float inertiaY = (float)(W * particle.Velocity.Y);

                float selfConfidenceX = (float)(c1 * rnd.NextDouble() * (particle.pBestPosition.X - particle.Position.X));
                float selfConfidenceY = (float)(c1 * rnd.NextDouble() * (particle.pBestPosition.Y - particle.Position.Y));

                float swarmConfidenceX = (float)(c2 * rnd.NextDouble() * (gBestPosition.X - particle.Position.X));
                float swarmConfidenceY = (float)(c2 * rnd.NextDouble() * (gBestPosition.Y - particle.Position.Y));

                float newVelocityX = inertiaX + selfConfidenceX + swarmConfidenceX;
                float newVelocityY = inertiaY + selfConfidenceY + swarmConfidenceY;

                particle.Velocity = new PointF(newVelocityX, newVelocityY);
                
                //particle.Position = new PointF(particle.Position.X + particle.Velocity.X, particle.Position.Y + particle.Velocity.Y);
                particle.Update();
            }
        }

        // Visualiza el estado actual de las partículas
        private void DrawParticles(Graphics g)
        {
            g.Clear(Color.White);

            // Obtener el centro del PictureBox
            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            // Fuente para las coordenadas
            Font font = new Font("Arial", 8);
            Brush textBrush = Brushes.Black;

            // Aplicar transformaciones
            g.ScaleTransform(zoomFactor, zoomFactor);
            g.TranslateTransform(offset.X / zoomFactor, offset.Y / zoomFactor); // Desplazamiento inverso

            // Dibujar cada partícula, ajustando la posición para que el centro sea (0,0)
            foreach (var particle in particles)
            {
                float adjustedX = centerX + particle.Position.X;
                float adjustedY = centerY - particle.Position.Y; // Invertir Y para coordenadas de pantalla
                g.FillEllipse(Brushes.Red, adjustedX - 5, adjustedY - 5, 10, 10);

                // Dibujar coordenadas al lado de cada partícula
                string coordinates = $"({particle.Position.X}, {particle.Position.Y})";
                g.DrawString(coordinates, font, textBrush, adjustedX + 10, adjustedY - 10);
            }

            // Dibujar la posición global mejorada (gBestPosition), también ajustada
            float gBestX = centerX + gBestPosition.X;
            float gBestY = centerY - gBestPosition.Y;
            g.FillEllipse(Brushes.Blue, gBestX - 5, gBestY - 5, 10, 10);

            // Dibujar coordenadas de gBestPosition
            string gBestCoordinates = $"({gBestPosition.X}, {gBestPosition.Y})";
            g.DrawString(gBestCoordinates, font, textBrush, gBestX + 10, gBestY - 10);
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Cambiar el factor de zoom
            if (e.Delta > 0)
                zoomFactor *= 1.1f; // Acercar
            else
                zoomFactor /= 1.1f; // Alejar

            pictureBox1.Invalidate(); // Redibujar el PictureBox
        }
        private bool isDragging = false;
        private PointF lastMousePos;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePos = e.Location;
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Calcular el nuevo desplazamiento
                offset.X += (e.X - lastMousePos.X) / zoomFactor;
                offset.Y += (e.Y - lastMousePos.Y) / zoomFactor;
                lastMousePos = e.Location;

                pictureBox1.Invalidate(); // Redibujar el PictureBox
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < n_iterations; i++)
            {
                UpdateBestValues();
                UpdateParticles();
                pictureBox1.Invalidate(); // Actualiza la visualización
                lblMejorResultado.Text = $"Mejor Valor: {gBestValue} en posición {gBestPosition}";
                Application.DoEvents(); // Permite que el UI se actualice durante la ejecución
                if (Math.Abs(gBestValue - 1) <= target_error) break;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawParticles(e.Graphics);
            DrawAxes(e.Graphics);

        }

        private void DrawAxes(Graphics g)
        {
            // Obtener el centro del PictureBox
            int centerX = pictureBox1.Width / 2;
            int centerY = pictureBox1.Height / 2;

            using (Pen pen = new Pen(Color.Black, 1))
            {
                // Línea vertical (eje Y)
                g.DrawLine(pen, centerX, 0, centerX, pictureBox1.Height);

                // Línea horizontal (eje X)
                g.DrawLine(pen, 0, centerY, pictureBox1.Width, centerY);
            }
        }

        public class Particle
        {
            public PointF Position { get; set; }
            public PointF Velocity { get; set; }
            public PointF pBestPosition { get; set; }
            public double pBestValue { get; set; }

            public Particle(Random rnd)
            {
                float x = (float)((rnd.NextDouble() * 776) * (rnd.Next(2) == 0 ? -1 : 1));
                float y = (float)((rnd.NextDouble() * 384) * (rnd.Next(2) == 0 ? -1 : 1));
                Position = new PointF(x,y);
                pBestPosition = Position;
                pBestValue = double.MaxValue;
                Velocity = new PointF(0, 0);                                
            }
            public void Update()
            {
                // Actualiza la posición sumando la velocidad
                Position = new PointF(Position.X + Velocity.X, Position.Y + Velocity.Y);
            }
        }
    }
}
