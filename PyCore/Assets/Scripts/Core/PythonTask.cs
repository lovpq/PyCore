using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// описывает одну задачу на Python для игрока
    /// 
    /// что содержит этот класс:
    /// - Название и описание задачи
    /// - Пример решения и ожидаемый код
    /// - Ожидаемый вывод программы (для проверки)
    /// - Награды за выполнение (опыт и деньги)
    /// - Состояние выполнения и сохраненный код игрока
    /// 
    /// как используется:
    /// - TaskManager создает список задач, используя этот класс
    /// - SimplePythonUI отображает задачи игроку
    /// - CheckOutput() проверяет правильность решения
    /// - savedCode хранит код игрока между сессиями
    /// 
    /// важно:
    /// - [Serializable] позволяет сохранять задачу в файл
    /// - [TextArea] создает многострочное поле в Inspector
    /// </summary>
    [Serializable] // делает класс сериализуемым (можно сохранить)
    public class PythonTask
    {
        // === ОПИСАНИЕ ЗАДАЧИ ===
        // название задачи (например: "Задача 1: Hello World")
        public string taskTitle;
        // описание задачи (что нужно сделать)
        // [TextArea(3, 10)] создает поле с минимум 3 и максимум 10 строками в Inspector
        [TextArea(3, 10)]
        public string taskDescription;
        
        // === ПРИМЕРЫ И РЕШЕНИЯ ===
        // пример кода для подсказки игроку
        [TextArea(3, 10)]
        public string taskExample;
        // правильное решение задачи (для справки)
        [TextArea(5, 15)]
        public string expectedCode;
        
        // === ПРОВЕРКА РЕШЕНИЯ ===
        // ожидаемый вывод программы (то, что должно вывести print())
        public string expectedOutput;
        
        // === НАГРАДЫ ===
        // сколько опыта (XP) получит игрок за выполнение
        public int rewardXP;
        // сколько денег получит игрок за выполнение
        public int rewardMoney;
        
        // === СОСТОЯНИЕ ===
        // завершена ли эта задача игроком
        public bool isCompleted;
        // код, который написал игрок (сохраняется при закрытии окна)
        public string savedCode;

        /// <summary>
        /// конструктор для создания задачи (базовая версия)
        /// используется, если не нужно указывать пример и ожидаемый вывод
        /// </summary>
        /// <param name="title">Название задачи</param>
        /// <param name="description">Описание задачи</param>
        /// <param name="code">Ожидаемый код решения</param>
        /// <param name="xp">Награда в опыте</param>
        /// <param name="money">Награда в деньгах</param>
        public PythonTask(string title, string description, string code, int xp, int money)
        {
            // инициализируем все поля
            taskTitle = title;
            taskDescription = description;
            expectedCode = code;
            rewardXP = xp;
            rewardMoney = money;
            // по умолчанию задача не выполнена
            isCompleted = false;
            // сохраненный код пустой
            savedCode = "";
            // пример и ожидаемый вывод не указаны
            taskExample = "";
            expectedOutput = "";
        }

        /// <summary>
        /// конструктор для создания задачи (полная версия)
        /// включает пример и ожидаемый вывод для автоматической проверки
        /// </summary>
        /// <param name="title">Название задачи</param>
        /// <param name="description">Описание задачи</param>
        /// <param name="example">Пример кода для подсказки</param>
        /// <param name="code">Ожидаемый код решения</param>
        /// <param name="output">Ожидаемый вывод программы</param>
        /// <param name="xp">Награда в опыте</param>
        /// <param name="money">Награда в деньгах</param>
        public PythonTask(string title, string description, string example, string code, string output, int xp, int money)
        {
            // инициализируем все поля
            taskTitle = title;
            taskDescription = description;
            taskExample = example;
            expectedCode = code;
            expectedOutput = output;
            rewardXP = xp;
            rewardMoney = money;
            // по умолчанию задача не выполнена
            isCompleted = false;
            // сохраненный код пустой
            savedCode = "";
        }

        /// <summary>
        /// Нормализует строку для сравнения: убирает \r, тримит каждую строку и весь текст.
        /// </summary>
        private static string NormalizeOutput(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            
            // убираем \r, разбиваем на строки
            string[] lines = text.Replace("\r", "").Split('\n');
            
            // тримим каждую строку справа (пробелы в конце строки не важны)
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd();
            }
            
            // собираем обратно и тримим весь текст
            return string.Join("\n", lines).Trim();
        }

        /// <summary>
        /// проверяет, правильный ли ВЫВОД (результат) выдала программа игрока.
        /// сравнивается только output программы, не код!
        /// игрок может написать любой код — главное чтобы вывод совпал.
        /// </summary>
        /// <param name="actualOutput">Фактический вывод программы игрока</param>
        /// <returns>true, если вывод совпадает с ожидаемым</returns>
        public bool CheckOutput(string actualOutput)
        {
            if (string.IsNullOrEmpty(expectedOutput))
            {
                UnityEngine.Debug.Log("[PythonTask] CheckOutput: expectedOutput пуст!");
                return false;
            }

            string expected = NormalizeOutput(expectedOutput);
            string actual = NormalizeOutput(actualOutput);

            UnityEngine.Debug.Log($"[PythonTask] CheckOutput сравнение:");
            UnityEngine.Debug.Log($"[PythonTask]   Ожидаемый (длина {expected.Length}): '{expected}'");
            UnityEngine.Debug.Log($"[PythonTask]   Полученный (длина {actual.Length}): '{actual}'");
            
            // сравниваем нормализованный вывод
            bool result = expected.Equals(actual, StringComparison.Ordinal);
            
            UnityEngine.Debug.Log($"[PythonTask]   Результат: {result}");
            
            if (!result)
            {
                // показываем первое различие для отладки
                int minLen = Math.Min(expected.Length, actual.Length);
                for (int i = 0; i < minLen; i++)
                {
                    if (expected[i] != actual[i])
                    {
                        UnityEngine.Debug.Log($"[PythonTask]   Первое различие на позиции {i}: ожидается '{expected[i]}' ({(int)expected[i]}), получено '{actual[i]}' ({(int)actual[i]})");
                        break;
                    }
                }
                if (expected.Length != actual.Length)
                {
                    UnityEngine.Debug.Log($"[PythonTask]   Разная длина: ожидается {expected.Length}, получено {actual.Length}");
                }
            }
            
            return result;
        }
    }
}
