# FastPreview

> Parallel preview-mesh pre-warming and a compute/display profiler for Grasshopper.
>
> 为 Grasshopper 提供的并行预览网格预热工具，以及一个计算/显示性能分析器。

https://github.com/user-attachments/assets/32ef205a-01a4-41cb-857d-9f8e0ac0a9b4

---

## What it does · 功能简介

In Grasshopper, the first time a Brep/Surface result is drawn in a viewport, Rhino has to build its preview mesh on the spot, which can make the first redraw after a solution feel slow. FastPreview adds two opt-in features:

- **Parallel Preview** — After every solution, it races ahead of Grasshopper and builds the preview meshes for Brep/Surface-like results **in parallel** (using the document's current mesh settings), before GH would otherwise build them serially on the first draw. By the time you look at the viewport, the meshes are already done.

  This helps when a scene contains **many objects at once** — e.g. one component spitting out 1000 spheres — where the per-object meshing adds up. It does **not** speed up a single heavy object (one giant Brep still meshes on one thread).

- **Advanced Profiler** — Replaces Grasshopper's profiler with one that also reports the **first-frame display (preview-meshing) time**, not just the solution/compute time.

在 Grasshopper 里，Brep/Surface 结果第一次在视口绘制时，Rhino 需要现场生成预览网格，因此一次运算后的首次重绘会显得卡。FastPreview 提供两个可选功能（默认关闭）：

- **Parallel Preview（并行预览）** — 每次运算结束后，抢在 Grasshopper 之前，按文档当前的网格设置，**并行**地把 Brep/Surface 类结果的预览网格画好；否则这些网格会在首次绘制时被 GH 串行生成。等你看向视口时网格已经就绪。

  它主要针对场景中**一次生成大量物件**的情况——例如一个组件吐出 1000 个球体——这种逐个网格化累加起来很可观。对**单一复杂物体没有效果**（一个巨大的 Brep 仍然在单线程上网格化）。

- **Advanced Profiler（高级分析器）** — 用一个增强版分析器替换 GH 自带的，除了运算/计算时间，还会显示**首帧显示（预览网格化）耗时**。

---

## Install · 安装

1. Download the `.gha` for your Rhino from the [Releases](https://github.com/KRLi3/FastPreview/releases) page:
   - **`FastPreview-rh8-netcore.gha`** — Rhino 8 default (.NET Core). **Most people on Rhino 8 want this one.**
   - **`FastPreview-rh8-netfx.gha`** — Rhino 8 started with `/netfx` (legacy .NET Framework mode), **and Rhino 7** (which is .NET Framework only).
2. In Grasshopper: **File ▸ Special Folders ▸ Components Folder**. Drop the `.gha` into that folder.
3. Right-click the `.gha` ▸ **Properties** ▸ tick **Unblock** ▸ OK. (Windows blocks downloaded DLLs by default.)
4. Restart Rhino.

1. 从 [Releases](https://github.com/KRLi3/FastPreview/releases) 页面下载对应你 Rhino 的 `.gha`：
   - **`FastPreview-rh8-netcore.gha`** — Rhino 8 默认（.NET Core）。**Rhino 8 用户大多用这个。**
   - **`FastPreview-rh8-netfx.gha`** — 用 `/netfx`（旧版 .NET Framework 模式）启动的 Rhino 8，**以及 Rhino 7**（Rhino 7 只有 .NET Framework）。
2. 在 Grasshopper 中：**File ▸ Special Folders ▸ Components Folder**，把 `.gha` 放进该文件夹。
3. 右键 `.gha` ▸ **属性** ▸ 勾选 **解除锁定（Unblock）** ▸ 确定。（Windows 默认会锁定下载来的 DLL。）
4. 重启 Rhino。

---

## Usage · 使用

Open the Grasshopper editor's **Display** menu. At the bottom you'll find two new toggles:

| Menu item · 菜单项 | Effect · 作用 |
|---|---|
| **Parallel Preview** | Parallel pre-warming of preview meshes after each solution. · 每次运算结束并行预热预览网格。 |
| **Advanced Profiler** | The profiler that also shows first-frame display time. · 额外显示首帧显示耗时的分析器。 |

Both are checkboxes — click to toggle. Your choices are remembered between sessions (saved next to the `.gha` as `FastPreview.settings.txt`).

两者都是勾选项，点击即可切换。你的选择会被记住（保存在 `.gha` 同目录下的 `FastPreview.settings.txt`）。

---

## Notes · 说明

- The parallel pre-warming respects the document's own mesh settings — it does not change the quality of your meshes, only *when* they're built.
- It skips hidden objects, locked components, and results that already have a preview mesh, so the cost on an unchanged document is minimal.
- 并行预热遵循文档自身的网格设置——它不改变网格质量，只改变网格**何时**被构建。
- 它会跳过隐藏对象、锁定的组件，以及已经有预览网格的结果，所以对未改动的文档几乎没有额外开销。

---

## License · 许可

[MIT](LICENSE) © 2026 Keran Li
