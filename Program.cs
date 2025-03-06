using System.Runtime.ExceptionServices;
using System.Xml.Linq;

public enum GradeType
{
    bad = 2,
    medium = 3,
    good = 4,
    great = 5
}
public class Student
{
    public string Name { get; private set; }

    public Dictionary<string, List<GradeType>> Grades { get; private set; }

    public Student(string name)
    {
        Name = name;
        Grades = new Dictionary<string, List<GradeType>>();
    }

    public void AddGrade(string course, GradeType grade)
    {
        if (!Grades.ContainsKey(course))
            Grades[course] = new List<GradeType>();

        Grades[course].Add(grade);
    }

    public double GetAverageGrade()
    {
        var allGrades = Grades.Values.SelectMany(g => g).Select(g => (int)g).ToList();

        return allGrades.Count > 0 ? allGrades.Average() : 0;
    }

    public void RenameStudent(string newName)
    {
        Name = newName;
    }

    public void RemoveCourse(string course)
    {
        if (Grades.ContainsKey(course))
        {
            Grades.Remove(course);
        }
    }

}

public interface IGrading
{
    void AddStudent(string name);
    void AddGrade(string studentName, string course, GradeType grade);
    void ShowStudentGrades(string studentName);
    void ShowTopStudents();

    event Action<string, string, GradeType> GradeAdded;


    void RemoveStudent(string name);

    void FindStudentWithGrade(GradeType grade);

    void RenameStudent(string oldName, string newName);

    void RemoveCourse(string course);

    void ShowBestAndWorstStudent();

}

public class GradeSystem : IGrading
{
    private Dictionary<string, Student> students = new();
    public event Action<string, string, GradeType> GradeAdded;
   
    public void AddStudent(string name)
    {
        if (!students.ContainsKey(name))
        {
            students[name] = new Student(name);
            Console.WriteLine($"Студент {name} добавлен");
        }
        else
            Console.WriteLine($"Студент {name} уже существует");


    }

    public void AddGrade(string studentName, string course, GradeType grade)
    {
        if (!students.ContainsKey(studentName)) 
        {       
            Console.WriteLine("Студент не найден!");
            return;
        }
        var student = students[studentName];
        student.AddGrade(course, grade);
        GradeAdded?.Invoke(studentName, course, grade);
    }

    public void ShowStudentGrades(string studentName)
    {

        if (!students.ContainsKey(studentName))
        {
            Console.WriteLine("Студент не найден!");
            return;
        }

        var student = students[studentName];

        Console.WriteLine($"Оценки {student.Name}");
        foreach (string course in student.Grades.Keys)
        {
            Console.Write($"{course}: ");
            foreach (GradeType grade in student.Grades[course])
            {
                Console.Write($"{grade} ");
            }
            Console.WriteLine();
        }
        
    }

    public void ShowTopStudents()
    {
        var topStudents = students.Values.OrderByDescending(s => s.GetAverageGrade()).Take(3);
        Console.WriteLine("Топ 3 студента по среднему баллу:");
        foreach (var student in topStudents)
        {
            Console.WriteLine($"{student.Name} {student.GetAverageGrade()}");
        }
    }

    public void RemoveStudent(string name)
    {
        if (!students.ContainsKey(name))
        {
            Console.WriteLine($"Студент {name} не найден");
            return;
        }
        students.Remove(name);
        Console.WriteLine($"Студент {name} удалён");

    }

    public void FindStudentWithGrade(GradeType grade)
    {
        foreach (Student s in students.Values) 
        {
            var grades = s.Grades.Values.SelectMany(g => g).ToList();
            if (grades.Contains(grade))
            {
                Console.WriteLine(s.Name);
            }
        }
    }

    public void RenameStudent(string oldName, string newName)
    {
        if (students.ContainsKey(newName))
        {
            Console.WriteLine($"Невозможно переименовать. Студент {newName} уже существует.");
            return;
        }

        if (!students.ContainsKey(oldName))
        {
            Console.WriteLine($"Студент {oldName} не найден");
            return;
        }
        students[oldName].RenameStudent(newName);
        students.Add(newName,students[oldName]);
        RemoveStudent(oldName);
        Console.WriteLine($"Студент {oldName} переименован в {newName}");
    }

    public void RemoveCourse(string course)
    {
        foreach (Student student in students.Values)
        {
            student.RemoveCourse(course);
        }

        Console.WriteLine($"Курс `{course}` удалён");
    }

    public void ShowBestAndWorstStudent()
    {
        var topStudents = students.Values.OrderByDescending(s => s.GetAverageGrade());
        var worstStudent = topStudents.Last();
        var bestStudent = topStudents.First();

        Console.WriteLine($"Лучший студент по среднему баллу: {bestStudent.Name}  {bestStudent.GetAverageGrade()}");
        Console.WriteLine($"Худший студент по среднему баллу: {worstStudent.Name}  {worstStudent.GetAverageGrade()}");

    }
}

public class NotificationSystem
{
    public void OnGradeAdded(string studentName, string course, GradeType grade)
    {
        Console.WriteLine($"[Уведомление] Новая оценка для {studentName} по {course}: {grade}");
    }
}

class Program
{
    static void Main()
    {
        IGrading gradeSystem = new GradeSystem();
        NotificationSystem notification = new();

        gradeSystem.GradeAdded += notification.OnGradeAdded;

        AddTestData(gradeSystem);

        while (true)
        { 
            ShowMenu();
            string choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Введите имя студента: ");
                    gradeSystem.AddStudent(Console.ReadLine());
                    break;

                case "2":
                    Console.Write("Введите имя студента: ");
                    string studentName = Console.ReadLine();
                    Console.Write("Введите курс: ");
                    string course = Console.ReadLine();
                    Console.WriteLine("Введите оценку (bad, average, good, great): ");
                    if (Enum.TryParse(Console.ReadLine(), out GradeType grade))
                        gradeSystem.AddGrade(studentName, course, grade);
                    else
                    {
                        Console.WriteLine("Некорректный ввод");
                    }
                    break;

                case "3":
                    Console.Write("Введите имя студента: ");
                    string studentName3 = Console.ReadLine();
                    gradeSystem.ShowStudentGrades(studentName3);
                    break;

                case "4":
                    gradeSystem.ShowTopStudents();
                    break;
                case "5":
                    Console.Write("Введите имя студента: ");
                    gradeSystem.RemoveStudent(Console.ReadLine());
                    break;
                case "6":
                    Console.Write("Введите название оценки: ");
                    if (Enum.TryParse(Console.ReadLine(), out GradeType grade6))
                        gradeSystem.FindStudentWithGrade(grade6);
                    else
                    {
                        Console.WriteLine("Некорректный ввод");
                    }
                    break;
                case "7":
                    Console.Write("Введите имя студента: ");
                    string oldName = Console.ReadLine();
                    Console.Write("Введите новое имя студента: ");
                    gradeSystem.RenameStudent(oldName,Console.ReadLine());
                    break;
                case "8":
                    Console.Write("Введите название курса: ");
                    gradeSystem.RemoveCourse(Console.ReadLine());
                    break;
                case "9":
                    gradeSystem.ShowBestAndWorstStudent();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Некорректный ввод. Попробуйте ещё раз.");
                    break;
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("\nМеню");
            Console.WriteLine("1. Добавить студента");
            Console.WriteLine("2. Выставить оценку студенту");
            Console.WriteLine("3. Показать оценки студента");
            Console.WriteLine("4. Показать топ 3 студентов");
            Console.WriteLine("5. Удалить студента");
            Console.WriteLine("6. Получить всех студентов с оценкой");
            Console.WriteLine("7. Переименовать студента");
            Console.WriteLine("8. Удалить курс");
            Console.WriteLine("9. Показать лучших и худших студентов");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите опцию: "); 
        }


        static void AddTestData(IGrading gradeSystem)
        {
            gradeSystem.AddStudent("Алексей");
            gradeSystem.AddStudent("Мария");
            gradeSystem.AddStudent("Иван");

            gradeSystem.AddGrade("Алексей", "Математика", GradeType.great);
            gradeSystem.AddGrade("Алексей", "Физика", GradeType.bad);
            gradeSystem.AddGrade("Мария", "Математика", GradeType.bad);
            gradeSystem.AddGrade("Мария", "Химия", GradeType.medium);
            gradeSystem.AddGrade("Иван", "Физика", GradeType.good);
            gradeSystem.AddGrade("Иван", "Химия", GradeType.great);
            Console.WriteLine("Добавлены тестовые данные");
            Console.WriteLine("");

            Console.WriteLine("Проверка данных:\n");

            gradeSystem.ShowStudentGrades("Алексей");
            Console.WriteLine();

            gradeSystem.ShowStudentGrades("Мария");
            Console.WriteLine();

            gradeSystem.ShowStudentGrades("Иван");
            Console.WriteLine();

            gradeSystem.ShowTopStudents();
            Console.WriteLine();
        }

    }
}
