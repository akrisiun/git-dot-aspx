﻿#region License

// Copyright 2011 Linquize
//  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/Linquize/git-dot-aspx

#endregion

using GitAspx.Lib;
using GitAspx.ViewModels;
using GitSharp;

namespace GitAspx.Controllers
{
    public class DownloadViewController : WebBrowsingBaseController<DownloadViewModel>
    {
        public DownloadViewController(RepositoryService repositories) : base(repositories) { }

        public override void Browse()
        {
            if (Model.PathSegments.Length > 0)
            {
                AbstractObject loObject = Model.RootTree[string.Join("/", Model.PathSegments)];
                if (loObject is Leaf)
                    Model.File = loObject as Leaf;
                else if (loObject is Tree)
                    Model.Directory = loObject as Tree;                    
            }
        }
    }
}
