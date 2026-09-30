"use client";

import { useState } from "react";
import type { Todo } from "@/entities/todo/model/types";

export function useTodos() {
  const [todos, setTodos] = useState<Todo[]>([]);

  const addTodo = (text: string) => {
    const trimmedText = text.trim();

    if (!trimmedText) {
      return false;
    }

    const id = crypto.randomUUID();

    setTodos((current) => [
      ...current,
      { id, text: trimmedText, completed: false },
    ]);
    return true;
  };

  const toggleTodo = (id: string) => {
    setTodos((current) =>
      current.map((todo) =>
        todo.id === id ? { ...todo, completed: !todo.completed } : todo,
      ),
    );
  };

  const deleteTodo = (id: string) => {
    setTodos((current) => current.filter((todo) => todo.id !== id));
  };

  const remainingCount = todos.filter((todo) => !todo.completed).length;

  return { todos, remainingCount, addTodo, toggleTodo, deleteTodo };
}
