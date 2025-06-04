﻿using Microsoft.Graph;
using System;
using System.Collections.Generic;

namespace Demo.Models
{
    [Serializable]
    public class FilesViewModel
    {
        public string DriveId { get; set; }
        public List<DriveItem> Path { get; set; }
    }
}
