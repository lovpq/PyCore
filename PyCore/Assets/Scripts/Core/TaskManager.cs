using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// менеджер задач на Python — 15 задач (5 на локацию, 3 локации)
    ///
    /// В Unity: создать GameObject "TaskManager", повесить этот скрипт.
    /// </summary>
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        public const int TASKS_PER_LOCATION = 5;
        public const int TOTAL_LOCATIONS = 3;

        [SerializeField] private List<PythonTask> tasks = new List<PythonTask>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeTasks();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeTasks()
        {
            tasks.Clear();

            // ===== ЛОКАЦИЯ 1: ОСНОВЫ (5 задач) =====

            tasks.Add(new PythonTask(
                "Задача 1: Hello World",
                "Напиши программу, которая выводит 'Hello, World!' на экран.",
                "print('Hello, World!')",
                "print('Hello, World!')",
                "Hello, World!",
                10, 50
            ));

            tasks.Add(new PythonTask(
                "Задача 2: Калькулятор",
                "Сложи числа 5 и 3 и выведи результат. Ожидаемый вывод: 8",
                "a = 5\nb = 3\nprint(a + b)",
                "print(5 + 3)",
                "8",
                15, 75
            ));

            tasks.Add(new PythonTask(
                "Задача 3: Цикл for",
                "Выведи числа от 1 до 5, каждое на новой строке.",
                "for i in range(1, 6):\n    print(i)",
                "for i in range(1, 6):\n    print(i)",
                "1\n2\n3\n4\n5",
                20, 100
            ));

            tasks.Add(new PythonTask(
                "Задача 4: Условие",
                "Проверь: 10 > 5? Если да — выведи 'Да', иначе 'Нет'.",
                "if 10 > 5:\n    print('Да')\nelse:\n    print('Нет')",
                "if 10 > 5:\n    print('Да')\nelse:\n    print('Нет')",
                "Да",
                20, 100
            ));

            tasks.Add(new PythonTask(
                "Задача 5: Список",
                "Создай список [1, 2, 3, 4, 5] и выведи его.",
                "numbers = [1, 2, 3, 4, 5]\nprint(numbers)",
                "print([1, 2, 3, 4, 5])",
                "[1, 2, 3, 4, 5]",
                25, 125
            ));

            // ===== ЛОКАЦИЯ 2: ФУНКЦИИ И СТРУКТУРЫ (5 задач) =====

            tasks.Add(new PythonTask(
                "Задача 6: Функция",
                "Напиши функцию greet(имя), которая выводит 'Привет, Игрок!'. Вызови greet('Игрок').",
                "def greet(name):\n    print(f'Привет, {name}!')\n\ngreet('Игрок')",
                "def greet(name):\n    print(f'Привет, {name}!')\ngreet('Игрок')",
                "Привет, Игрок!",
                30, 150
            ));

            tasks.Add(new PythonTask(
                "Задача 7: Словарь",
                "Создай словарь {'name': 'Python'} и выведи значение ключа 'name'.",
                "my_dict = {'name': 'Python'}\nprint(my_dict['name'])",
                "print({'name': 'Python'}['name'])",
                "Python",
                35, 175
            ));

            tasks.Add(new PythonTask(
                "Задача 8: Цикл while",
                "Выведи числа от 0 до 3 через while, каждое на новой строке.",
                "i = 0\nwhile i < 4:\n    print(i)\n    i += 1",
                "i = 0\nwhile i < 4:\n    print(i)\n    i += 1",
                "0\n1\n2\n3",
                40, 200
            ));

            tasks.Add(new PythonTask(
                "Задача 9: List comprehension",
                "Создай список квадратов чисел от 1 до 5 и выведи его.\nОжидаемый вывод: [1, 4, 9, 16, 25]",
                "squares = [x**2 for x in range(1, 6)]\nprint(squares)",
                "print([x**2 for x in range(1, 6)])",
                "[1, 4, 9, 16, 25]",
                45, 225
            ));

            tasks.Add(new PythonTask(
                "Задача 10: Строки",
                "Создай строку 'Python' и выведи её заглавными буквами.\nОжидаемый вывод: PYTHON",
                "text = 'Python'\nprint(text.upper())",
                "print('Python'.upper())",
                "PYTHON",
                45, 225
            ));

            // ===== ЛОКАЦИЯ 3: ПРОДВИНУТЫЕ (5 задач) =====

            tasks.Add(new PythonTask(
                "Задача 11: Класс",
                "Создай класс Person с методом say_hello(). Вывод: 'Привет, я Игрок'",
                "class Person:\n    def __init__(self, name):\n        self.name = name\n    def say_hello(self):\n        print(f'Привет, я {self.name}')\n\np = Person('Игрок')\np.say_hello()",
                "class Person:\n    def __init__(self, name):\n        self.name = name\n    def say_hello(self):\n        print(f'Привет, я {self.name}')\np = Person('Игрок')\np.say_hello()",
                "Привет, я Игрок",
                50, 250
            ));

            tasks.Add(new PythonTask(
                "Задача 12: Lambda и map",
                "Удвой каждое число в списке [1,2,3] через map/lambda.\nОжидаемый вывод: [2, 4, 6]",
                "result = list(map(lambda x: x*2, [1,2,3]))\nprint(result)",
                "print(list(map(lambda x: x*2, [1,2,3])))",
                "[2, 4, 6]",
                55, 275
            ));

            tasks.Add(new PythonTask(
                "Задача 13: Сортировка",
                "Отсортируй список [3,1,4,1,5,9,2,6] и выведи результат.",
                "nums = [3,1,4,1,5,9,2,6]\nprint(sorted(nums))",
                "print(sorted([3,1,4,1,5,9,2,6]))",
                "[1, 1, 2, 3, 4, 5, 6, 9]",
                55, 275
            ));

            tasks.Add(new PythonTask(
                "Задача 14: Словарь comprehension",
                "Создай словарь {1:1, 2:4, 3:9, 4:16, 5:25} через dict comprehension.\nОжидаемый вывод: {1: 1, 2: 4, 3: 9, 4: 16, 5: 25}",
                "d = {x: x**2 for x in range(1,6)}\nprint(d)",
                "print({x: x**2 for x in range(1,6)})",
                "{1: 1, 2: 4, 3: 9, 4: 16, 5: 25}",
                60, 300
            ));

            tasks.Add(new PythonTask(
                "Задача 15: Финал",
                "Напиши функцию fizzbuzz(n), которая:\n- для кратных 3 выводит 'Fizz'\n- для кратных 5 выводит 'Buzz'\n- для кратных 15 выводит 'FizzBuzz'\n- иначе само число\nВызови для чисел 1-15.",
                "for i in range(1,16):\n    if i%15==0: print('FizzBuzz')\n    elif i%3==0: print('Fizz')\n    elif i%5==0: print('Buzz')\n    else: print(i)",
                "for i in range(1,16):\n    if i%15==0: print('FizzBuzz')\n    elif i%3==0: print('Fizz')\n    elif i%5==0: print('Buzz')\n    else: print(i)",
                "1\n2\nFizz\n4\nBuzz\nFizz\n7\n8\nFizz\nBuzz\n11\nFizz\n13\n14\nFizzBuzz",
                70, 350
            ));
        }

        // === ПУБЛИЧНЫЕ МЕТОДЫ ===

        public List<PythonTask> GetTasks() => tasks;

        /// <summary>Получить задачи для конкретной локации (0, 1, 2)</summary>
        public List<PythonTask> GetTasksForLocation(int locationIndex)
        {
            int start = locationIndex * TASKS_PER_LOCATION;
            int end = Mathf.Min(start + TASKS_PER_LOCATION, tasks.Count);
            return tasks.GetRange(start, end - start);
        }

        /// <summary>Номер локации для задачи (0, 1, 2)</summary>
        public int GetLocationForTask(int taskIndex)
        {
            return taskIndex / TASKS_PER_LOCATION;
        }

        /// <summary>Название локации</summary>
        public string GetLocationName(int locationIndex)
        {
            switch (locationIndex)
            {
                case 0: return "Подвал";
                case 1: return "Квартира";
                case 2: return "Офис";
                default: return $"Локация {locationIndex + 1}";
            }
        }

        public void CompleteTask(int taskIndex)
        {
            if (taskIndex >= 0 && taskIndex < tasks.Count && !tasks[taskIndex].isCompleted)
            {
                tasks[taskIndex].isCompleted = true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerData.AddExperience(tasks[taskIndex].rewardXP);
                    GameManager.Instance.PlayerData.AddMoney(tasks[taskIndex].rewardMoney);
                }

                // Проверяем, завершена ли локация
                if (LocationManager.Instance != null)
                {
                    LocationManager.Instance.CheckLocationProgress();
                }
            }
        }

        public int GetCompletedTasksCount()
        {
            int count = 0;
            foreach (var task in tasks) if (task.isCompleted) count++;
            return count;
        }

        /// <summary>Сколько задач завершено для локации</summary>
        public int GetCompletedTasksForLocation(int locationIndex)
        {
            int start = locationIndex * TASKS_PER_LOCATION;
            int end = Mathf.Min(start + TASKS_PER_LOCATION, tasks.Count);
            int count = 0;
            for (int i = start; i < end; i++)
                if (tasks[i].isCompleted) count++;
            return count;
        }

        /// <summary>Завершена ли локация</summary>
        public bool IsLocationCompleted(int locationIndex)
        {
            return GetCompletedTasksForLocation(locationIndex) >= TASKS_PER_LOCATION;
        }

        public void SaveTaskCode(int taskIndex, string code)
        {
            if (taskIndex >= 0 && taskIndex < tasks.Count)
                tasks[taskIndex].savedCode = code;
        }
    }
}
