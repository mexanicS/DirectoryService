"use client";

import { useState, type FormEvent } from "react";
import Link from "next/link";
import { useTodos } from "@/features/todo/model/useTodos";
import { routes } from "@/shared/config/routes";
import { Button } from "@/shared/ui/button";

export function TodoPlayground() {
  const [draft, setDraft] = useState("");
  const { todos, remainingCount, addTodo, toggleTodo, deleteTodo } = useTodos();

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (addTodo(draft)) {
      setDraft("");
    }
  };

  return (
    <main className="flex min-h-screen w-full justify-center bg-slate-50 px-4 py-12 text-slate-900 sm:py-20">
      <section className="w-full max-w-2xl">
        <header className="mb-8">
          <Link
            href={routes.home}
            className="mb-6 inline-block text-sm font-medium text-indigo-600 hover:underline"
          >
            ← На главную
          </Link>
          <p className="mb-2 text-sm font-semibold uppercase tracking-widest text-indigo-600">
            Практика React
          </p>
          <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">
            Список задач
          </h1>
          <p className="mt-3 text-slate-600">
            Добавьте задачу, отметьте выполненную или удалите её.
          </p>
        </header>

        <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-7">
          <form onSubmit={handleSubmit} className="flex flex-col gap-3 sm:flex-row">
            <label htmlFor="new-todo" className="sr-only">
              Новая задача
            </label>
            <input
              id="new-todo"
              type="text"
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              placeholder="Что нужно сделать?"
              className="min-w-0 flex-1 rounded-lg border border-slate-300 px-4 py-3 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
            <Button
              type="submit"
              disabled={!draft.trim()}
              className="h-auto min-h-12 rounded-lg bg-indigo-600 px-5 py-3 text-base font-medium text-white hover:bg-indigo-700 disabled:cursor-not-allowed disabled:bg-slate-300"
            >
              Добавить
            </Button>
          </form>

          <div className="mt-8 border-b border-slate-200 pb-3">
            <h2 className="font-semibold">
              {remainingCount} невыполненных
            </h2>
          </div>

          {todos.length === 0 ? (
            <p className="py-10 text-center text-slate-500">
              Пока нет задач. Добавьте первую выше.
            </p>
          ) : (
            <ul className="divide-y divide-slate-100">
              {todos.map((todo) => (
                <li key={todo.id} className="flex items-center gap-3 py-4">
                  <input
                    type="checkbox"
                    checked={todo.completed}
                    onChange={() => toggleTodo(todo.id)}
                    aria-label={
                      (todo.completed ? "Снять отметку с задачи «" : "Отметить задачу «") +
                      todo.text +
                      "»"
                    }
                    className="h-5 w-5 shrink-0 accent-indigo-600"
                  />
                  <span
                    className={
                      "min-w-0 flex-1 break-words " +
                      (todo.completed
                        ? "text-slate-400 line-through"
                        : "text-slate-800")
                    }
                  >
                    {todo.text}
                  </span>
                  <button
                    type="button"
                    onClick={() => deleteTodo(todo.id)}
                    aria-label={"Удалить задачу «" + todo.text + "»"}
                    className="shrink-0 rounded-lg px-3 py-2 text-sm font-medium text-rose-600 transition hover:bg-rose-50 focus-visible:outline-2 focus-visible:outline-rose-600"
                  >
                    Удалить
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      </section>
    </main>
  );
}
