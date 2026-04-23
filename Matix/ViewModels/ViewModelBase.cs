using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Matix.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Викликає подію PropertyChanged для сповіщення про зміну властивості.
        /// </summary>
        /// <param name="propertyName">Назва властивості, що змінилася.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Встановлює значення поля та викликає подію зміни властивості, якщо значення змінилося.
        /// </summary>
        /// <typeparam name="T">Тип властивості.</typeparam>
        /// <param name="field">Посилання на поле, яке потрібно оновити.</param>
        /// <param name="value">Нове значення.</param>
        /// <param name="propertyName">Назва властивості.</param>
        /// <returns>True, якщо значення було змінено; інакше false.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
