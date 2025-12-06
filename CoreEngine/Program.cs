using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace SmallEngineCS
{

    //? Punto de entrada de la aplicación
    public static class Program

    {
        //? Crea la ventana del juego y ejecuta el bucle principal
        public static void Main(string[] args)
        {
            using (Game game = new Game(1920, 1080, "Small Engine CS"))
            {
                game.Run();
            }
        }
    }

    //? Clase principal que maneja la ventana de juego y la lógica de renderizado
    public class Game : GameWindow
    {
        private Shader shader = null!;  //? Shader compilado para renderizar
        private int VertexArrayObject;  //? Identificador del Vertex Array Object

        //? Vectores de vertices para un triangulo 3D
        float[] vertices = {
            // positions        
             0.0f,  0.5f, 0.0f,  // top middle
             0.5f, -0.5f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  // bottom left
        };

        //? Constructor: inicializa la ventana con dimensiones y título
        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = new Vector2i(width, height), Title = title })
        {
            // Configuración de la ventana realizada en el constructor base
            ClientSize = new Vector2i(width, height);
            Title = title;
            WindowState = WindowState.Normal;
        }

        //? Esta funcion se ejecuta en cada frame para actualizar la logica del juego
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            //? Cerrar la ventana si se presiona Escape
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            //? Presionar F11 para alternar pantalla completa
            if (KeyboardState.IsKeyPressed(Keys.F11))
            {
                WindowState = WindowState == WindowState.Fullscreen
                    ? WindowState.Normal
                    : WindowState.Fullscreen;
            }
        }



        //? Esta funcion se ejecuta una vez al cargar el juego
        protected override void OnLoad()
        {
            base.OnLoad();

            //? Establecer color de fondo (RGB + Alpha)
            GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);

            //? ID del Vertex Buffer Object - almacena datos de vertices en la GPU
            int VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject); //? Activar el buffer
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw); //? Copiar datos de vertices al GPU

            //? Cargar y compilar los shaders (vertex y fragment)
            shader = new Shader("CoreEngine/shader/shader.vert", "CoreEngine/shader/shader.frag");

            //? Crear y configurar el Vertex Array Object (VAO)
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject); //? Activar el VAO

            //? Configurar atributo de posición (layout location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            //? Vincular buffer y cargar datos de vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            //? Configurar atributo de posición nuevamente (redundante pero necesario para el VAO)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            //? Activar el shader program
            shader.Use();

        }

        //? Esta función se ejecuta cada frame para renderizar la escena
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            //? Limpiar el buffer de color con el color de fondo establecido
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //? Activar el shader program
            shader.Use();
            //? Vincular el VAO que contiene la geometría del triángulo
            GL.BindVertexArray(VertexArrayObject);
            //? Dibujar el triángulo (3 vertices)
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            //? Intercambiar buffers para mostrar el frame renderizado
            SwapBuffers();
        }

        //? Se ejecuta cuando cambia el tamaño de la ventana
        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            //? Actualizar viewport para que coincida con las nuevas dimensiones
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        //? Se ejecuta cuando la ventana se cierra para liberar recursos
        protected override void OnUnload()
        {
            base.OnUnload();

            //? Liberar recursos del shader (GPU memory)
            shader.Dispose();
        }

    }

    //? Clase para manejar shaders - compila y linkea programas de GPU
    public class Shader
    {
        int Handle;           //? Identificador del programa de shader compilado
        int VertexShader;     //? Identificador del vertex shader
        int FragmentShader;   //? Identificador del fragment shader
        public Shader(string vertexPath, string fragmentPath)
        {
            //? Leer los archivos de los shaders
            string VertexShaderSource = File.ReadAllText(vertexPath);
            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            //? Crear los shaders
            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            //? Compilar los shaders y verificar errores
            GL.CompileShader(VertexShader);
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int sussess);

            if (sussess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.WriteLine($"Error al compilar el Vertex Shader: {infoLog}");
            }

            GL.CompileShader(FragmentShader);
            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out sussess);

            if (sussess == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.WriteLine($"Error al compilar el Fragment Shader: {infoLog}");
            }

            //? Enlazar los shaders en un programa
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out sussess);
            if (sussess == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine($"Error al enlazar el shader: {infoLog}");
            }

            //? Limpiar los shaders individuales ya que están compilados en el programa
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);


        }

        //? Implementacion de IDisposable para liberar recursos GPU
        private bool disposedValue = false;

        //? Activar este programa de shader para renderizar
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        //? Liberar recursos de GPU - patrón IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //? Eliminar el programa de shader de GPU
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }

        //? Destructor - detecta si se olvidó liberar recursos manualmente
        ~Shader()
        {
            if (disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
            }
        }

        //? Método público para liberar recursos
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}