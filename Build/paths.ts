import { $ } from "bun";
import { resolve } from "path";

const kitsRoot = "C:\\Program Files (x86)\\Windows Kits\\10\\bin";

export async function findSigntool(): Promise<string> {
  // Use cmd /c dir to avoid Bun's $ shell mangling PowerShell's $_ variables
  const result = await $`cmd /c dir /b /ad "${kitsRoot}"`.quiet().nothrow();
  const dirs = result.text().trim().split("\n").map((s) => s.trim()).filter((s) => /^\d+\./.test(s));
  dirs.sort((a, b) => b.localeCompare(a, undefined, { numeric: true }));

  for (const dir of dirs) {
    const candidate = resolve(kitsRoot, dir, "x64", "signtool.exe");
    if (await Bun.file(candidate).exists()) {
      return candidate;
    }
  }

  throw new Error(`signtool.exe not found under ${kitsRoot}`);
}
