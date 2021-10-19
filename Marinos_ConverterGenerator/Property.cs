using System;
using Microsoft.Practices.Prism.ViewModel;

namespace Marinos_ConverterGenerator
{
    [Serializable]
    public class Property : NotificationObject
    {
        private int    _id;
        private string _name;
        
        public int Id
        {
            get => _id;
            set { _id = value; RaisePropertyChanged(() => Id); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; RaisePropertyChanged(() => Name);}
        }

        public Property()
        {
        }

        public Property(int id)
        {
            Id = id;
        }

        public Property(Property old)
        {
            if (old == null) return;
            Id   = old.Id;
            Name = old.Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}