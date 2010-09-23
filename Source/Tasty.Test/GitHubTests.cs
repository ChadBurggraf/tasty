using System;
using System.IO;
using Microsoft.Build.BuildEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.GitHub;

namespace Tasty.Test
{
    [TestClass]
    public class GitHubTests
    {
        #region WebhookSamplePayload

        private const string WebhookSamplePayload = @"{
          ""before"": ""5aef35982fb2d34e9d9d4502f6ede1072793222d"",
          ""repository"": {
            ""url"": ""http://github.com/defunkt/github"",
            ""name"": ""github"",
            ""description"": ""You're lookin' at it."",
            ""watchers"": 5,
            ""forks"": 2,
            ""private"": 1,
            ""owner"": {
              ""email"": ""chris@ozmm.org"",
              ""name"": ""defunkt""
            }
          },
          ""commits"": [
            {
              ""id"": ""41a212ee83ca127e3c8cf465891ab7216a705f59"",
              ""url"": ""http://github.com/defunkt/github/commit/41a212ee83ca127e3c8cf465891ab7216a705f59"",
              ""author"": {
                ""email"": ""chris@ozmm.org"",
                ""name"": ""Chris Wanstrath""
              },
              ""message"": ""okay i give in"",
              ""timestamp"": ""2008-02-15T14:57:17-08:00"",
              ""added"": [""filepath.rb""]
            },
            {
              ""id"": ""de8251ff97ee194a289832576287d6f8ad74e3d0"",
              ""url"": ""http://github.com/defunkt/github/commit/de8251ff97ee194a289832576287d6f8ad74e3d0"",
              ""author"": {
                ""email"": ""chris@ozmm.org"",
                ""name"": ""Chris Wanstrath""
              },
              ""message"": ""update pricing a tad"",
              ""timestamp"": ""2008-02-15T14:36:34-08:00""
            }
          ],
          ""after"": ""de8251ff97ee194a289832576287d6f8ad74e3d0"",
          ""ref"": ""refs/heads/master""
        }";

        #endregion

        [TestMethod]
        public void GitHubWebhook_DeSerialize()
        {
            var hook = GitHubWebhook.Deserialize(WebhookSamplePayload);
            Assert.IsNotNull(hook);
            Assert.IsFalse(String.IsNullOrEmpty(hook.Repository.Name));
            Assert.IsFalse(String.IsNullOrEmpty(hook.Repository.Owner.Name));
            Assert.AreEqual(2, hook.Commits.Length);
        }

        [TestMethod]
        public void GitHubWebhookExecuter_Execute()
        {
            GitHubWebhookMSBuildExecuter executer = new GitHubWebhookMSBuildExecuter("GitHubWebhookBefore.proj", GitHubWebhook.Deserialize(WebhookSamplePayload));
            executer.LogFile = Path.GetRandomFileName().WithExtension("log");
            Assert.IsTrue(executer.Execute());
            Assert.IsTrue(File.ReadAllText(executer.LogFile).Contains("Hello, github!", StringComparison.Ordinal));
        }

        [TestMethod]
        public void GitHubWebhookExecuter_PrepareProject()
        {
            Engine engine = new Engine();
            Project project = new Project(engine);
            project.Load("GitHubWebHookBefore.proj");

            GitHubWebhookMSBuildExecuter.PrepareProject(project, GitHubWebhook.Deserialize(WebhookSamplePayload));
            string outputProjectFile = Path.GetRandomFileName();
            project.Save(outputProjectFile);

            Assert.AreEqual(File.ReadAllText("GitHubWebHookAfter.proj"), File.ReadAllText(outputProjectFile));
        }
    }
}
