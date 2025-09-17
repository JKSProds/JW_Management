namespace JW_Management.Models;

public class AppContext
{
    public TenantContext _tenantContext { get; set; }

    public Tenant _currentTenant => _manualTenant > 0
        ? _tenantContext._tenants.Where(t => t.Id == _manualTenant).First()
        : _tenantContext._currentTenant;

    public DbContext _dbContext { get; set; }
    public FileContext _fileContext { get; set; }
    public JWApiContext _jwApiContext { get; set; }

    public int _manualTenant { get; set; }

    public AppContext(TenantContext tenantContext, int manualTenant = 0)
    {
        _tenantContext = tenantContext;
        _dbContext = new DbContext(this);
        _fileContext = new FileContext(this);
        _jwApiContext = new JWApiContext(this);
        _jwApiContext = new JWApiContext(this);
        _manualTenant = manualTenant;
    }
}