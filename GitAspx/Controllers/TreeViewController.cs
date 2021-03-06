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

using System;
using GitAspx.Lib;
using GitAspx.ViewModels;
using GitSharp;

namespace GitAspx.Controllers
{
    public class TreeViewController : WebBrowsingBaseController<TreeViewModel>
    {
        public TreeViewController(RepositoryService repositories) : base(repositories) { }

        public override void Browse()
        {
            Tree loTree = Model.RootTree;
            Tree loTree2 = null;
            if (Model.PathSegments.Length > 0)
                loTree2 = Model.RootTree[string.Join("/", Model.PathSegments)] as Tree;
            if (loTree2 != null)
                loTree = loTree2;
            Model.Directories = loTree.Trees;
            Model.Files = loTree.Leaves;
        }
    }
}
