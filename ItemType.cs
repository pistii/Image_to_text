using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Image_to_text
{
    public class ItemType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ItemType()
        {
            ItemTypes = new ObservableCollection<ItemModel>
            {
                new ItemModel() { Id = 0, Name = "Txt" },
                new ItemModel() { Id = 1, Name = "Pdf" },
                new ItemModel() { Id = 2, Name = "Word" }
            };
            TranslateFrom = new ObservableCollection<TranslateType>
            {
                new TranslateType() { Id = 0, Language = "Hun" },
                new TranslateType() { Id = 1, Language = "Eng"}
            };
            SelectedItemType = null;
        }

        public ObservableCollection<ItemModel> ItemTypes { get; set; }
        public ObservableCollection<TranslateType> TranslateFrom { get; set; }

        private TranslateType _selectedTranslateType;
        private ItemModel _selectedItemType;

        public TranslateType SelectedTranslateType
        {
            get { return _selectedTranslateType; }
            set
            {
                if (_selectedTranslateType != value)
                {
                    _selectedTranslateType = value;
                    OnPropertyChanged(nameof(SelectedTranslateType));
                }
            }
        }

        public ItemModel SelectedItemType
        {
            get { return _selectedItemType; }
            set
            {
                if (_selectedItemType != value)
                {
                    _selectedItemType = value;
                    OnPropertyChanged(nameof(SelectedItemType));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TranslateType
    {
        public int Id { get; set; }
        public string Language { get; set; }
    }
}
