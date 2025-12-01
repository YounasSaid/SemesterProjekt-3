-- Aktivér RLS (safe hvis allerede aktiv)
ALTER TABLE public.quizzes ENABLE ROW LEVEL SECURITY;

-- SELECT: må kun læse egne
DROP POLICY IF EXISTS quizzes_select_own ON public.quizzes;
CREATE POLICY quizzes_select_own
ON public.quizzes
AS PERMISSIVE
FOR SELECT
TO authenticated
USING (created_by = auth.uid());

-- INSERT: quiz skal tilhøre den oprettende bruger
DROP POLICY IF EXISTS quizzes_insert_own ON public.quizzes;
CREATE POLICY quizzes_insert_own
ON public.quizzes
AS PERMISSIVE
FOR INSERT
TO authenticated
WITH CHECK (created_by = auth.uid());

-- UPDATE: må kun ændre egne
DROP POLICY IF EXISTS quizzes_update_own ON public.quizzes;
CREATE POLICY quizzes_update_own
ON public.quizzes
AS PERMISSIVE
FOR UPDATE
TO authenticated
USING (created_by = auth.uid());

-- DELETE: må kun slette egne (du har den, men vi sikrer idempotens)
DROP POLICY IF EXISTS quizzes_delete_own ON public.quizzes;
CREATE POLICY quizzes_delete_own
ON public.quizzes
AS PERMISSIVE
FOR DELETE
TO authenticated
USING (created_by = auth.uid());
