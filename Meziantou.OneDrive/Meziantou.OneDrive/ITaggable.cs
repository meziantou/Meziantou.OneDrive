using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meziantou.OneDrive
{
    public interface ITaggable
    {
        bool TagsEnabled { get; }
        int TagCount { get; }
        Task<IEnumerable<Tag>> GetTagsAsync(GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken));

        Task<Tag> AddTagAsync(User user, float x, float y, CancellationToken cancellationToken = default(CancellationToken));
    }
}