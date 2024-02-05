namespace WebApplication3.Model {
    public class AuditLogService {
        private readonly AuthDbContext _context;

        public AuditLogService(AuthDbContext context) {
            _context = context;
        }

        public async Task LogActionAsync(string userID, string sessionID, AuditLog.ActionType action) {
            var auditRecord = new AuditLog {
                UserID = userID,
                SessionID = sessionID,
                Action = action
            };

            _context.AuditLogs?.Add(auditRecord);
            await _context.SaveChangesAsync();
        }
    }
}
