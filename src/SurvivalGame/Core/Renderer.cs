// 화면을 그리는 컴포넌트만 이 클래스를 상속합니다.
abstract class Renderer : Behaviour
{
	public int SortingLayer { get; set; }
	public int Order { get; set; }
	public abstract void Render(Camera camera);
}
