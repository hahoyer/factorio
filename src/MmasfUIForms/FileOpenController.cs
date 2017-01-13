using System;
using System.Collections.Generic;
using System.Linq;

namespace MmasfUIForms
{
    sealed class FileOpenController : IFileOpenController
    {
        readonly string DefaultDirectory;

        internal FileOpenController(string defaultDirectory)
        {
            DefaultDirectory = defaultDirectory;
        }

        string IFileOpenController.FileName { get; set; }

        string IFileOpenController.CreateEmptyFile => "";

        string IFileOpenController.DefaultDirectory => DefaultDirectory;
    }
}