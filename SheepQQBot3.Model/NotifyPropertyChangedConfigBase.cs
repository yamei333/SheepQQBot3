using System;
using MessagePack;

namespace SheepQQBot3.Model
{
    [MessagePackObject]
    public abstract class NotifyPropertyChangedConfigBase : NotifyPropertyChangedBase
    {
        [Key(nameof(Id))]
        public Guid Id { get; set; }
    }
}