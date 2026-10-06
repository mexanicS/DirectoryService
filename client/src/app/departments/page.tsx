import type { Metadata } from "next";
import { SectionPlaceholder } from "@/widgets/section-placeholder";

export const metadata: Metadata = {
  title: "Подразделения | Directory Service",
};

export default function DepartmentsPage() {
  return <SectionPlaceholder title="Подразделения" />;
}
