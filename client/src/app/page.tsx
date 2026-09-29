import { Card, CardDescription, CardHeader, CardTitle } from "@/shared/ui/card";

export default function Home() {
  return (
    <main className="flex min-h-screen w-full items-center justify-center bg-muted/30 px-4 py-12">
      <Card className="w-full max-w-lg shadow-sm">
        <CardHeader className="gap-3">
          <CardTitle>
            <h1 className="text-3xl font-semibold tracking-tight">
              Directory Service
            </h1>
          </CardTitle>
          <CardDescription className="text-base">
            Тут будут разделы
          </CardDescription>
        </CardHeader>
      </Card>
    </main>
  );
}
