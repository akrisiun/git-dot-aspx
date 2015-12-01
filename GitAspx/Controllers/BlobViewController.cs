#region License

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
using System.Text;
using GitAspx.Lib;
using GitAspx.ViewModels;
using GitSharp;

namespace GitAspx.Controllers
{
    public class BlobViewController : WebBrowsingBaseController<BlobViewModel>
    {
        public BlobViewController(RepositoryService repositories) : base(repositories) { }

        public override void Browse()
        {
            Leaf loLeaf = null;
            if (Model.PathSegments.Length > 0)
            {
                var path = string.Join("/", Model.PathSegments);
                if (path.EndsWith(".cs.html"))
                    path = path.Replace(".html", string.Empty);

                loLeaf = Model.RootTree[path] as Leaf;
            }
            //if (loLeaf != null && loLeaf.Name.EndsWith(".cs.html"))
            //    loLeaf.ClearPath(".html");

            Model.File = loLeaf;
        }
    }
}
