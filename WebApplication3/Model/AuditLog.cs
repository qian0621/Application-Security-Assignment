namespace WebApplication3.Model
{
    public class AuditLog {
        public int ID { get; set; }
        public string UserID { get; set; }
        public string SessionID { get; set; }
        public ActionType Action { get; set; } = ActionType.None;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public enum ActionType {
            None = 0,
            Register = 1,
            Login = 2,
            Logout = 3
        }
    }
}
