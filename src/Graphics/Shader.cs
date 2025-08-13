using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace MukiaEngine.Graphics;

public class Shader : IDisposable
{
	public int Handle;
	public int VertexShader;
	public int FragmentShader;

	private readonly Dictionary<string, int> _uniformLocations;

	public void Use()
	{
		GL.UseProgram(Handle);
	}

	private bool disposedValue = false;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			GL.DeleteProgram(Handle);

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	~Shader()
	{
		if (disposedValue == false)
		{
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
		}
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(Handle, attribName);
	}

	public void SetInt(string name, int data)
	{
		GL.UseProgram(Handle);
		GL.Uniform1(_uniformLocations[name], data);
	}

	/// <summary>
	/// Set a uniform float on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void SetFloat(string name, float data)
	{
		GL.UseProgram(Handle);
		GL.Uniform1(_uniformLocations[name], data);
	}

	/// <summary>
	/// Set a uniform Matrix4 on this shader
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	/// <remarks>
	///   <para>
	///   The matrix is transposed before being sent to the shader.
	///   </para>
	/// </remarks>
	public void SetMatrix4(string name, Matrix4 data)
	{
		GL.UseProgram(Handle);
		GL.UniformMatrix4(_uniformLocations[name], true, ref data);
	}

	public void SetColor(string name, Color data)
	{
		GL.UseProgram(Handle);
		GL.Uniform4(_uniformLocations[name], data.R, data.G, data.B, 0f);
	}

	/// <summary>
	/// Set a uniform Vector3 on this shader.
	/// </summary>
	/// <param name="name">The name of the uniform</param>
	/// <param name="data">The data to set</param>
	public void SetVector3(string name, GLVector3 data)
	{
		GL.UseProgram(Handle);
		GL.Uniform3(_uniformLocations[name], ref data);
	}

	private static void CompileShader(int shader)
	{
		// Try to compile the shader
		GL.CompileShader(shader);

		// Check for compilation errors
		GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
		if (code != (int)All.True)
		{
			// We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
			var infoLog = GL.GetShaderInfoLog(shader);
			throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
		}
	}

	private static void LinkProgram(int program)
	{
		// We link the program
		GL.LinkProgram(program);

		// Check for linking errors
		GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
		if (code != (int)All.True)
		{
			// We can use `GL.GetProgramInfoLog(program)` to get information about the error.
			throw new Exception($"Error occurred whilst linking Program({program})");
		}
	}

	internal void SetMatrix4(string v, object model)
	{
		throw new NotImplementedException();
	}

	public Shader(string vertPath, string fragPath)
	{
		var shaderSource = File.ReadAllText(vertPath);
		var vertexShader = GL.CreateShader(ShaderType.VertexShader);
		GL.ShaderSource(vertexShader, shaderSource);
		CompileShader(vertexShader);

		shaderSource = File.ReadAllText(fragPath);
		var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		GL.ShaderSource(fragmentShader, shaderSource);
		CompileShader(fragmentShader);

		Handle = GL.CreateProgram();
		GL.AttachShader(Handle, vertexShader);
		GL.AttachShader(Handle, fragmentShader);

		LinkProgram(Handle);

		GL.DetachShader(Handle, vertexShader);
		GL.DetachShader(Handle, fragmentShader);
		GL.DeleteShader(fragmentShader);
		GL.DeleteShader(vertexShader);

		GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

		_uniformLocations = [];

		for (var i = 0; i < numberOfUniforms; i++)
		{
			var key = GL.GetActiveUniform(Handle, i, out _, out _);
			var location = GL.GetUniformLocation(Handle, key);

			_uniformLocations.Add(key, location);
		}
	}
}