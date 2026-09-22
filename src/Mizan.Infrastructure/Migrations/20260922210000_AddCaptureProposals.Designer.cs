using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Mizan.Infrastructure.Persistence;

#nullable disable

namespace Mizan.Infrastructure.Migrations;

[DbContext(typeof(MizanDbContext))]
[Migration("20260922210000_AddCaptureProposals")]
public partial class AddCaptureProposals
{
}
