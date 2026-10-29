using Microsoft.AspNetCore.Http;

namespace QuestMaster.Rules;

// Deliberate violation of NFR-25 to show that the architecture tests fail. Reverted in the next commit.
public static class ForbiddenDependencyDemo
{
    public static string ReadFile(string path) => File.ReadAllText(path);

    public static string RequestPath(HttpContext context) => context.Request.Path;
}
