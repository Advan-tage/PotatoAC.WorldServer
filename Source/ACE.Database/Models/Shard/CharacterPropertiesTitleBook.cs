﻿using System;
using System.Collections.Generic;

namespace ACE.Database.Models.Shard
{
    public partial class CharacterPropertiesTitleBook
    {
        public uint Id { get; set; }
        public uint CharacterId { get; set; }
        public uint TitleId { get; set; }

        public Character Character { get; set; }
    }
}