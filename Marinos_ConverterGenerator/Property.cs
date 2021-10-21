using System;
using Microsoft.Practices.Prism.ViewModel;

namespace Marinos_ConverterGenerator
{
    [Serializable]
    public class Property : NotificationObject
    {
        private int    _id;
        private string _name;
        private string _type;
        private bool   _nullable;

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

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged(() => Type);
            }
        }

        public bool Nullable
        {
            get => _nullable;
            set { _nullable = value; RaisePropertyChanged(() => Nullable);}
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
            Id       = old.Id;
            Name     = old.Name;
            Type     = old.Type;
            Nullable = old.Nullable;
        }

        public override string ToString()
        {
            return $"{Name} {Type}{(Nullable ? " NULL" : "")}";
        }
    }
}