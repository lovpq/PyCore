using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

/// <summary>
/// выполняет Python код из Unity
/// 
/// что делает этот скрипт:
/// - Запускает Python код и программы из Unity
/// - Получает вывод программы (результаты print)
/// - Получает ошибки выполнения
/// - Сообщает о завершении выполнения через события (events)
/// 
/// требования:
/// - Python должен быть установлен на компьютере
/// - Python должен быть добавлен в PATH системы
/// </summary>
public class PythonExecutor : MonoBehaviour
{
    private const string PYTHON_COMMAND = "python";
    
    // Таймаут выполнения Python в миллисекундах (10 секунд)
    private const int EXECUTION_TIMEOUT_MS = 10000;

    public event Action<string> OnOutputReceived;
    public event Action<string> OnErrorReceived;
    public event Action<int> OnExecutionCompleted;

    /// <summary>
    /// выполняет Python код из строки.
    /// Код передаётся через stdin, что избегает проблем с экранированием кавычек.
    /// Выполнение ограничено таймаутом, чтобы не заморозить Unity.
    /// </summary>
    public void ExecutePythonCode(string code)
    {
        try
        {
            Process process = new Process();
            
            process.StartInfo.FileName = PYTHON_COMMAND;
            // Передаём код через stdin (флаг "-" означает читать из stdin)
            // -u отключает буферизацию вывода
            process.StartInfo.Arguments = "-u -";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    output.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    error.AppendLine(args.Data);
                }
            };

            process.Start();
            
            // Пишем код в stdin и закрываем поток
            process.StandardInput.Write(code);
            process.StandardInput.Close();
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            // Ждём с таймаутом, чтобы не заморозить Unity
            bool exited = process.WaitForExit(EXECUTION_TIMEOUT_MS);
            
            if (!exited)
            {
                try { process.Kill(); } catch { }
                OnErrorReceived?.Invoke("Ошибка: время выполнения истекло (10 секунд). Возможно, в коде бесконечный цикл.");
                OnExecutionCompleted?.Invoke(-1);
                process.Close();
                return;
            }

            // Тримим вывод, чтобы убрать trailing newline от AppendLine
            string outputText = output.ToString().TrimEnd();
            string errorText = error.ToString().TrimEnd();

            if (!string.IsNullOrEmpty(outputText))
            {
                OnOutputReceived?.Invoke(outputText);
            }

            if (!string.IsNullOrEmpty(errorText))
            {
                OnErrorReceived?.Invoke(errorText);
            }

            OnExecutionCompleted?.Invoke(process.ExitCode);
            process.Close();
        }
        catch (Exception ex)
        {
            OnErrorReceived?.Invoke($"Ошибка запуска Python: {ex.Message}\nУбедитесь, что Python установлен и добавлен в PATH.");
            OnExecutionCompleted?.Invoke(-1);
        }
    }

    /// <summary>
    /// выполняет Python файл
    /// </summary>
    public void ExecutePythonFile(string filePath)
    {
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = PYTHON_COMMAND;
            process.StartInfo.Arguments = $"-u \"{filePath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    output.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    error.AppendLine(args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            bool exited = process.WaitForExit(EXECUTION_TIMEOUT_MS);
            
            if (!exited)
            {
                try { process.Kill(); } catch { }
                OnErrorReceived?.Invoke("Ошибка: время выполнения истекло.");
                OnExecutionCompleted?.Invoke(-1);
                process.Close();
                return;
            }

            string outputText = output.ToString().TrimEnd();
            string errorText = error.ToString().TrimEnd();

            if (!string.IsNullOrEmpty(outputText))
            {
                OnOutputReceived?.Invoke(outputText);
            }

            if (!string.IsNullOrEmpty(errorText))
            {
                OnErrorReceived?.Invoke(errorText);
            }

            OnExecutionCompleted?.Invoke(process.ExitCode);
            process.Close();
        }
        catch (Exception ex)
        {
            OnErrorReceived?.Invoke($"Ошибка запуска Python файла: {ex.Message}\nУбедитесь, что Python установлен и добавлен в PATH.");
            OnExecutionCompleted?.Invoke(-1);
        }
    }
}
