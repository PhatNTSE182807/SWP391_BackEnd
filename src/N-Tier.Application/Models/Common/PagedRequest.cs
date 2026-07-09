using System.ComponentModel;

namespace N_Tier.Application.Models;

public class PagedRequest
{
    private int _page = 1;
    protected int _size = 10;

    [DefaultValue(1)]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    [DefaultValue(10)]
    public virtual int Size
    {
        get => _size;
        set => _size = value < 1 ? 10 : (value > 100 ? 100 : value);
    }
}

public class TopicPagedRequest : PagedRequest
{
    public TopicPagedRequest()
    {
        _size = 100;
    }

    [DefaultValue(100)]
    public override int Size
    {
        get => _size;
        set => _size = value < 1 ? 100 : (value > 100 ? 100 : value);
    }
}

public class JournalPagedRequest : PagedRequest
{
    public JournalPagedRequest()
    {
        _size = 100;
    }

    [DefaultValue(100)]
    public override int Size
    {
        get => _size;
        set => _size = value < 1 ? 100 : (value > 100 ? 100 : value);
    }
}
