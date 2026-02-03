# Game-Test-Hyper-Casual
# Lê Bùi Hoàng Văn
# lebuihoangvan@gmail.com

1. Kiến trúc & Tổ chức Code (Architecture & Patterns)
    Hệ thống được xây dựng dựa trên mô hình MVC (biến thể cho Unity) kết hợp với các Design Patterns sau:
    -	Singleton Pattern (GameManager):
        •	Đóng vai trò là Centralized State Manager. Đảm bảo tính duy nhất của instance quản lý vòng đời game (Game Loop: Init, Gameplay, Win/Loss State). Giúp truy cập toàn cục dễ dàng (Instance) mà không cần truyền tham chiếu phức tạp.
    -	Builder Pattern (Map/Hole/Lane Builder):
        •	Tách biệt logic khởi tạo (Construction) khỏi logic biểu diễn (Representation). Các class này chịu trách nhiệm Procedural Generation (sinh map tự động) dựa trên tham số đầu vào, giúp hệ thống có tính linh hoạt cao (Scalability).
    -	Component Pattern (Unity Core):
        •	Các Controller (LaneController, TinyCharacter) hoạt động như các Independent Modules. Chúng tuân thủ nguyên tắc Single Responsibility Principle (Mỗi component chỉ làm một việc duy nhất).

2. Chiến lược mở rộng (Scalability Strategy)
    Để mở rộng lên 100 level, cần chuyển đổi từ Hard-coded Logic sang Data-Driven Design:
    -	ScriptableObject (Data Container):
        •	Tạo các file Asset lưu trữ cấu hình Level (LevelConfig). Mỗi file chứa metadata: MapType (Enum), MinionCount (Int), HoleData (List/Array).
    -	Dependency Injection:
        •  	GameManager sẽ không còn hard-code logic switch-case. Thay vào đó, nó sẽ nhận một LevelConfig làm tham số đầu vào và inject dữ liệu này xuống các Builder để khởi tạo màn chơi động (Dynamic Initialization).

3. Cân bằng Game (Game Balancing)
    Dựa trên Telemetry Data (giả định), nếu tỷ lệ Churn Rate (bỏ game) cao do độ khó, cần tinh chỉnh các biến số cân bằng (Balancing Variables) theo thứ tự:
    -	Time Constraint (TimeLimit): Nới lỏng giới hạn thời gian để giảm áp lực lên người chơi (Stress Threshold).
    -	Unit Velocity (RunSpeed): Giảm tốc độ di chuyển của Agent (TinyCharacter) để tăng cửa sổ phản xạ (Reaction Window) cho người chơi.
    -	Threshold Requirement (Gate Amount): Hạ thấp yêu cầu đầu vào của GateController, giảm độ khó trong việc quản lý tài nguyên (Resource Management).

4. Retention Mechanics
    Core Loop của game đánh vào hai yếu tố tâm lý học hành vi (Behavioral Psychology):
    -	Flow State & Boids Simulation:
        •	Cơ chế di chuyển của TinyCharacter (dựa trên thuật toán tìm đường và Boids-like behavior) tạo ra các chuyển động mượt mà, đồng bộ. Hiệu ứng thị giác này kích thích ASMR-like satisfaction, giữ người chơi trong trạng thái "Flow".
    -	Visual Feedback Loop & Closure:
        •	Cơ chế GateController (giảm số -> biến mất) tạo ra cảm giác Sense of Completion/Closure. Đây là một Positive Feedback Loop mạnh mẽ, thưởng cho não bộ dopamine mỗi khi một tác vụ (Task) được hoàn thành triệt để (Clean up).

