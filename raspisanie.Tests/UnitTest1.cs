using System;
using System.Collections.Generic;
using Xunit;
using raspisanie.Models;
using raspisanie.Services;
using raspisanie.MVVM.Memento;

namespace raspisanie.Tests
{
    public class SimpleUnitTests
    {
        // Тест 1: Считаем процент, если список занятий пустой (должен быть 0)
        [Fact]
        public void Test_CalculatePercentage_EmptyList_ReturnsZero()
        {
            // Arrange (Подготовка)
            var service = AttendanceService.Instance;
            var list = new List<Para>();

            // Act (Действие)
            double result = service.CalculatePercentage(list);

            // Assert (Проверка)
            Assert.Equal(0, result);
        }

        // Тест 2: Считаем процент, если нет ни одной отмеченной пары (все null, должен быть 0)
        [Fact]
        public void Test_CalculatePercentage_NoMarkedParas_ReturnsZero()
        {
            // Arrange
            var service = AttendanceService.Instance;
            var list = new List<Para>
            {
                new Para { Id = 1, Subject = "Математика", IsAttended = null },
                new Para { Id = 2, Subject = "Физика", IsAttended = null }
            };

            // Act
            double result = service.CalculatePercentage(list);

            // Assert
            Assert.Equal(0, result);
        }

        // Тест 3: Считаем процент посещаемости (1 был, 1 не был -> должно быть 50%)
        [Fact]
        public void Test_CalculatePercentage_HalfAttended_ReturnsFiftyPercent()
        {
            // Arrange
            var service = AttendanceService.Instance;
            var list = new List<Para>
            {
                new Para { Id = 1, Subject = "Информатика", IsAttended = true },
                new Para { Id = 2, Subject = "История", IsAttended = false }
            };

            // Act
            double result = service.CalculatePercentage(list);

            // Assert
            Assert.Equal(50.0, result);
        }

        // Тест 4: Проверяем, что изначально история изменений настроек пуста и CanUndo равен false
        [Fact]
        public void Test_SettingsCaretaker_InitiallyEmpty_CannotUndo()
        {
            // Arrange
            var caretaker = new SettingsCaretaker();

            // Act & Assert
            Assert.False(caretaker.CanUndo);
            Assert.Null(caretaker.Pop());
        }

        // Тест 5: Проверяем запись и чтение снимка настроек в Caretaker (Undo механизм)
        [Fact]
        public void Test_SettingsCaretaker_PushAndPop_WorksCorrectly()
        {
            // Arrange
            var caretaker = new SettingsCaretaker();
            var memento = new SettingsMemento("test_path.html", "Все занятия");

            // Act
            caretaker.Push(memento);

            // Assert
            Assert.True(caretaker.CanUndo);

            var poppedMemento = caretaker.Pop();
            Assert.NotNull(poppedMemento);
            Assert.Equal("test_path.html", poppedMemento.HtmlFilePath);
            Assert.Equal("Все занятия", poppedMemento.DefaultTab);
            Assert.False(caretaker.CanUndo);
        }
    }
}
