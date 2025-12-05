using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace SmallEngineCS
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (Game game = new Game(800, 600, "Small Engine CS"))
            {
                game.Run();



            }
        }
    }

    public class Game : GameWindow
    {
        Shader shader;

        //? Vectores de vertices para un triangulo 3D
        float[] vertices = {
            // positions        
             0.0f,  0.5f, 0.0f,  // top middle
             0.5f, -0.5f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  // bottom left
        };

        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { ClientSize = new Vector2i(width, height), Title = title })
        {

        }

        //? Esta funcion se ejecuta en cada frame para actualizar la logica del juego
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);



            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        //? Esta funcion se ejecuta una vez al cargar el juego
        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);

            //? ID del Vertex Buffer Object
            int VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject); // Bind del VBO
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw); // Carga de datos al VBO

            shader = new Shader("CoreEngine/shader/shader.vert", "CoreEngine/shader/shader.frag"); // Carga y compila los shaders

            int VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            shader.Use();

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);


            GL.Clear(ClearBufferMask.ColorBufferBit);


            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            shader.Dispose();
        }

    }



    //? Clase para manejar shaders
    public class Shader
    {
        int Handle;
        int VertexShader;
        int FragmentShader;
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

            //TODO: Limpiar los shaders ya que estan enlazados en el programa
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);


        }

        //? Implementacion de IDisposable para liberar recursos GPU
        private bool disposedValue = false;

        public void Use()
        {
            GL.UseProgram(Handle);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            if (disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}