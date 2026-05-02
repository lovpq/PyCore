using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Выполняет Python код из Unity.
/// Поддерживает python3 и python (автоопределение).
/// Результаты возвращаются через события на main thread.
/// </summary>
public class PythonExecutor : MonoBehaviour
{
    // На macOS/Linux чаще python3, на Windows — python
    private static readonly string[] PYTHON_CANDIDATES = { "python3", "python" };
    private static string resolvedPythonCommand = null;

    private const int EXECUTION_TIMEOUT_MS = 10000;

    public event Action<string> OnOutputReceived;
    public event Action<string> OnErrorReceived;
    public event Action<int> OnExecutionCompleted;

    private readonly System.Collections.Generic.Queue<Action> mainThreadQueue =
        new System.Collections.Generic.Queue<Action>();

    private void Awake()
    {
        if (resolvedPythonCommand == null)
            resolvedPythonCommand = FindPythonCommand();
    }

    private void Update()
    {
        lock (mainThreadQueue)
        {
            while (mainThreadQueue.Count > 0)
                mainThreadQueue.Dequeue().Invoke();
        }
    }

    /// <summary>
    /// Возвращает первую доступную команду Python из списка кандидатов.
    /// </summary>
    private static string FindPythonCommand()
    {
        foreach (string candidate in PYTHON_CANDIDATES)
        {
            try
            {
                var test = new Process();
                test.StartInfo.FileName = candidate;
                test.StartInfo.Arguments = "--version";
                test.StartInfo.UseShellExecute = false;
                test.StartInfo.RedirectStandardOutput = true;
                test.StartInfo.RedirectStandardError = true;
                test.StartInfo.CreateNoWindow = true;
                test.Start();
                test.WaitForExit(3000);
                test.Close();
                return candidate;
            }
            catch
            {
                Debug.LogWarning($"PythonExecutor: command '{candidate}' not available, trying next.");
            }
        }
        return PYTHON_CANDIDATES[0];
    }

    private void Dispatch(Action action)
    {
        lock (mainThreadQueue)
            mainThreadQueue.Enqueue(action);
    }

    /// <summary>
    /// Выполняет Python код в отдельном потоке, не блокируя Unity.
    /// </summary>
    public void ExecutePythonCode(string code)
    {
        ThreadPool.QueueUserWorkItem(_ => RunProcess(code));
    }

    private void RunProcess(string code)
    {
        try
        {
            string pythonCmd = resolvedPythonCommand ?? PYTHON_CANDIDATES[0];
            Process process = new Process();
            process.StartInfo.FileName = pythonCmd;
            // -X utf8 форсирует UTF-8 для stdin/stdout/stderr на Windows (Python 3.7+)
            process.StartInfo.Arguments = "-X utf8 -u -";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardInputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            // Переменная окружения как запасной вариант для старых версий Python
            process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
            process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    if (output.Length > 0) output.Append('\n');
                    output.Append(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    if (error.Length > 0) error.Append('\n');
                    error.Append(args.Data);
                }
            };

            process.Start();
            process.StandardInput.Write(code);
            process.StandardInput.Close();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            bool exited = process.WaitForExit(EXECUTION_TIMEOUT_MS);

            if (!exited)
            {
                try { process.Kill(); } catch { }
                process.Close();
                Dispatch(() =>
                {
                    OnErrorReceived?.Invoke("Ошибка: время выполнения истекло (10 секунд). Возможно, в коде бесконечный цикл.");
                    OnExecutionCompleted?.Invoke(-1);
                });
                return;
            }

            string outputText = output.ToString();
            string errorText = error.ToString();
            int exitCode = process.ExitCode;
            process.Close();

            Dispatch(() =>
            {
                if (!string.IsNullOrEmpty(outputText))
                    OnOutputReceived?.Invoke(outputText);
                if (!string.IsNullOrEmpty(errorText))
                    OnErrorReceived?.Invoke(errorText);
                OnExecutionCompleted?.Invoke(exitCode);
            });
        }
        catch (Exception ex)
        {
            Dispatch(() =>
            {
                OnErrorReceived?.Invoke($"Ошибка запуска Python: {ex.Message}\nУбедитесь, что Python установлен и добавлен в PATH.");
                OnExecutionCompleted?.Invoke(-1);
            });
        }
    }

    /// <summary>
    /// Выполняет Python файл в отдельном потоке, не блокируя Unity.
    /// </summary>
    public void ExecutePythonFile(string filePath)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                string pythonCmd = resolvedPythonCommand ?? PYTHON_CANDIDATES[0];
                Process process = new Process();
                process.StartInfo.FileName = pythonCmd;
                process.StartInfo.Arguments = $"-X utf8 -u \"{filePath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
                process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        if (output.Length > 0) output.Append('\n');
                        output.Append(args.Data);
                    }
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        if (error.Length > 0) error.Append('\n');
                        error.Append(args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool exited = process.WaitForExit(EXECUTION_TIMEOUT_MS);
                if (!exited)
                {
                    try { process.Kill(); } catch { }
                    process.Close();
                    Dispatch(() =>
                    {
                        OnErrorReceived?.Invoke("Ошибка: время выполнения истекло.");
                        OnExecutionCompleted?.Invoke(-1);
                    });
                    return;
                }

                string outputText = output.ToString().TrimEnd();
                string errorText = error.ToString().TrimEnd();
                int exitCode = process.ExitCode;
                process.Close();

                Dispatch(() =>
                {
                    if (!string.IsNullOrEmpty(outputText)) OnOutputReceived?.Invoke(outputText);
                    if (!string.IsNullOrEmpty(errorText)) OnErrorReceived?.Invoke(errorText);
                    OnExecutionCompleted?.Invoke(exitCode);
                });
            }
            catch (Exception ex)
            {
                Dispatch(() =>
                {
                    OnErrorReceived?.Invoke($"Ошибка запуска Python файла: {ex.Message}\nУбедитесь, что Python установлен и добавлен в PATH.");
                    OnExecutionCompleted?.Invoke(-1);
                });
            }
        });
    }
}
