��/ /   C o p y r i g h t   ( c )   2 0 0 4 - 2 0 0 8   M y S Q L   A B ,   2 0 0 8 - 2 0 0 9   S u n   M i c r o s y s t e m s ,   I n c .  
 / /  
 / /   M y S Q L   C o n n e c t o r / N E T   i s   l i c e n s e d   u n d e r   t h e   t e r m s   o f   t h e   G P L v 2  
 / /   < h t t p : / / w w w . g n u . o r g / l i c e n s e s / o l d - l i c e n s e s / g p l - 2 . 0 . h t m l > ,   l i k e   m o s t    
 / /   M y S Q L   C o n n e c t o r s .   T h e r e   a r e   s p e c i a l   e x c e p t i o n s   t o   t h e   t e r m s   a n d    
 / /   c o n d i t i o n s   o f   t h e   G P L v 2   a s   i t   i s   a p p l i e d   t o   t h i s   s o f t w a r e ,   s e e   t h e    
 / /   F L O S S   L i c e n s e   E x c e p t i o n  
 / /   < h t t p : / / w w w . m y s q l . c o m / a b o u t / l e g a l / l i c e n s i n g / f o s s - e x c e p t i o n . h t m l > .  
 / /  
 / /   T h i s   p r o g r a m   i s   f r e e   s o f t w a r e ;   y o u   c a n   r e d i s t r i b u t e   i t   a n d / o r   m o d i f y    
 / /   i t   u n d e r   t h e   t e r m s   o f   t h e   G N U   G e n e r a l   P u b l i c   L i c e n s e   a s   p u b l i s h e d    
 / /   b y   t h e   F r e e   S o f t w a r e   F o u n d a t i o n ;   v e r s i o n   2   o f   t h e   L i c e n s e .  
 / /  
 / /   T h i s   p r o g r a m   i s   d i s t r i b u t e d   i n   t h e   h o p e   t h a t   i t   w i l l   b e   u s e f u l ,   b u t    
 / /   W I T H O U T   A N Y   W A R R A N T Y ;   w i t h o u t   e v e n   t h e   i m p l i e d   w a r r a n t y   o f   M E R C H A N T A B I L I T Y    
 / /   o r   F I T N E S S   F O R   A   P A R T I C U L A R   P U R P O S E .   S e e   t h e   G N U   G e n e r a l   P u b l i c   L i c e n s e    
 / /   f o r   m o r e   d e t a i l s .  
 / /  
 / /   Y o u   s h o u l d   h a v e   r e c e i v e d   a   c o p y   o f   t h e   G N U   G e n e r a l   P u b l i c   L i c e n s e   a l o n g    
 / /   w i t h   t h i s   p r o g r a m ;   i f   n o t ,   w r i t e   t o   t h e   F r e e   S o f t w a r e   F o u n d a t i o n ,   I n c . ,    
 / /   5 1   F r a n k l i n   S t ,   F i f t h   F l o o r ,   B o s t o n ,   M A   0 2 1 1 0 - 1 3 0 1     U S A  
  
 u s i n g   S y s t e m ;  
 u s i n g   S y s t e m . D a t a ;  
 u s i n g   S y s t e m . I O ;  
 u s i n g   S y s t e m . G l o b a l i z a t i o n ;  
 u s i n g   S y s t e m . T h r e a d i n g ;  
 u s i n g   N U n i t . F r a m e w o r k ;  
 u s i n g   S y s t e m . T e x t ;  
  
 n a m e s p a c e   M y S q l . D a t a . M y S q l C l i e n t . T e s t s  
 {  
 	 [ T e s t F i x t u r e ]  
 	 p u b l i c   c l a s s   C h a r a c t e r S e t T e s t s   :   B a s e T e s t  
 	 {  
 	 	 [ T e s t ]  
 	 	 p u b l i c   v o i d   U s e F u n c t i o n s ( )  
 	 	 {  
 	 	 	 e x e c S Q L ( " C R E A T E   T A B L E   T e s t   ( v a l i d   c h a r ,   U s e r C o d e   v a r c h a r ( 1 0 0 ) ,   p a s s w o r d   v a r c h a r ( 1 0 0 ) )   C H A R S E T   l a t i n 1 " ) ;  
  
 	 	 	 M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n . C o n n e c t i o n S t r i n g   +   " ; c h a r s e t = l a t i n 1 " ) ;  
 	 	 	 c . O p e n ( ) ;  
 	 	 	 M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " S E L E C T   v a l i d   F R O M   T e s t   W H E R E   V a l i d   =   ' Y '   A N D   "   +  
 	 	 	 	 " U s e r C o d e   =   ' u s e r n a m e '   A N D   P a s s w o r d   =   A E S _ E N C R Y P T ( ' P a s s w o r d ' , ' a b c ' ) " ,   c ) ;  
 	 	 	 c m d . E x e c u t e S c a l a r ( ) ;  
 	 	 	 c . C l o s e ( ) ;  
 	 	 }  
  
                 [ T e s t ]  
                 p u b l i c   v o i d   V a r B i n a r y ( )  
                 {  
                         i f   ( V e r s i o n   <   n e w   V e r s i o n ( 4 ,   1 ) )   r e t u r n ;  
  
                         c r e a t e T a b l e ( " C R E A T E   T A B L E   t e s t   ( i d   i n t ,   n a m e   v a r c h a r ( 2 0 0 )   c o l l a t e   u t f 8 _ b i n )   c h a r s e t   u t f 8 " ,   " I n n o D B " ) ;  
                         e x e c S Q L ( " I N S E R T   I N T O   t e s t   V A L U E S   ( 1 ,   ' T e s t 1 ' ) " ) ;  
  
                         M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " S E L E C T   *   F R O M   t e s t " ,   c o n n ) ;  
                         u s i n g   ( M y S q l D a t a R e a d e r   r e a d e r   =   c m d . E x e c u t e R e a d e r ( ) )  
                         {  
                                 A s s e r t . I s T r u e ( r e a d e r . R e a d ( ) ) ;  
                                 o b j e c t   o   =   r e a d e r . G e t V a l u e ( 1 ) ;  
                                 A s s e r t . I s T r u e ( o   i s   s t r i n g ) ;  
                         }  
                 }  
  
 	 	 [ T e s t ]  
 	 	 p u b l i c   v o i d   L a t i n 1 C o n n e c t i o n ( )    
 	 	 {  
                         i f   ( V e r s i o n   <   n e w   V e r s i o n ( 4 ,   1 ) )   r e t u r n ;  
  
 	 	 	 e x e c S Q L ( " C R E A T E   T A B L E   T e s t   ( i d   I N T ,   n a m e   V A R C H A R ( 2 0 0 ) )   C H A R S E T   l a t i n 1 " ) ;  
 	 	 	 e x e c S Q L ( " I N S E R T   I N T O   T e s t   V A L U E S (   1 ,   _ l a t i n 1   ' T e s t ' ) " ) ;  
  
 	 	 	 M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n . C o n n e c t i o n S t r i n g   +   " ; c h a r s e t = l a t i n 1 " ) ;  
 	 	 	 c . O p e n ( ) ;  
  
 	 	 	 M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " S E L E C T   i d   F R O M   T e s t   W H E R E   n a m e   L I K E   ' T e s t ' " ,   c ) ;  
 	 	 	 o b j e c t   i d   =   c m d . E x e c u t e S c a l a r ( ) ;  
 	 	 	 A s s e r t . A r e E q u a l ( 1 ,   i d ) ;  
 	 	 	 c . C l o s e ( ) ;  
 	 	 }  
  
                 / / /   < s u m m a r y >  
                 / / /   B u g   # 1 1 6 2 1     	 c o n n e c t o r   d o e s   n o t   s u p p o r t   c h a r s e t   c p 1 2 5 0  
                 / / /   < / s u m m a r y >  
 / *                 [ T e s t ]  
                 p u b l i c   v o i d   C P 1 2 5 0 C o n n e c t i o n ( )  
                 {  
                         e x e c S Q L ( " C R E A T E   T A B L E   T e s t   ( n a m e   V A R C H A R ( 2 0 0 ) )   C H A R S E T   c p 1 2 5 0 " ) ;  
  
                         M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n . C o n n e c t i o n S t r i n g   +   " ; c h a r s e t = c p 1 2 5 0 " ) ;  
                         c . O p e n ( ) ;  
  
                         M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " I N S E R T   I N T O   T e s t   V A L U E S ( ' � d}a' ) " ,   c ) ;  
                         c m d . E x e c u t e N o n Q u e r y ( ) ;  
  
                         c m d . C o m m a n d T e x t   =   " S E L E C T   n a m e   F R O M   T e s t " ;  
                         o b j e c t   n a m e   =   c m d . E x e c u t e S c a l a r ( ) ;  
                         A s s e r t . A r e E q u a l ( " � d}a" ,   n a m e ) ;  
                         c . C l o s e ( ) ;  
                 }  
 * /  
                 / / /   < s u m m a r y >  
                 / / /   B u g   # 1 4 5 9 2   W r o n g   c o l u m n   l e n g t h   r e t u r n e d   f o r   V A R C H A R   U T F 8   c o l u m n s    
                 / / /   < / s u m m a r y >  
                 [ T e s t ]  
                 p u b l i c   v o i d   G e t S c h e m a O n U T F 8 ( )  
                 {  
                         i f   ( V e r s i o n . M a j o r   > =   6 )   r e t u r n ;  
  
                         e x e c S Q L ( " C R E A T E   T A B L E   T e s t ( n a m e   V A R C H A R ( 4 0 )   N O T   N U L L ,   n a m e 2   V A R C H A R ( 2 0 ) )   "   +  
                                 " C H A R A C T E R   S E T   u t f 8 " ) ;  
                         e x e c S Q L ( " I N S E R T   I N T O   T e s t   V A L U E S ( ' T e s t ' ,   ' T e s t ' ) " ) ;  
  
                         M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " S E L E C T   *   F R O M   t e s t " ,   c o n n ) ;  
                         u s i n g   ( M y S q l D a t a R e a d e r   r e a d e r   =   c m d . E x e c u t e R e a d e r ( ) )  
                         {  
                                 D a t a T a b l e   d t   =   r e a d e r . G e t S c h e m a T a b l e ( ) ;  
                                 A s s e r t . A r e E q u a l ( 4 0 ,   d t . R o w s [ 0 ] [ " C o l u m n S i z e " ] ) ;  
                                 A s s e r t . A r e E q u a l ( 2 0 ,   d t . R o w s [ 1 ] [ " C o l u m n S i z e " ] ) ;  
                         }  
                 }  
  
                 [ T e s t ]  
                 p u b l i c   v o i d   U T F 8 B l o g s T r u n c a t i n g ( )  
                 {  
                         e x e c S Q L ( " C R E A T E   T A B L E   t e s t   ( n a m e   L O N G T E X T )   C H A R S E T   u t f 8 " ) ;  
  
                         s t r i n g   s z P a r a m   =   " t e s t : � � � � � � " ;  
                         s t r i n g   s z S Q L   =   " I N S E R T   I N T O   t e s t   V a l u e s   ( ? m o n P a r a m e t r e ) " ;  
  
                         s t r i n g   c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; c h a r s e t = u t f 8 " ;  
                         u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
                         {  
                                 c . O p e n ( ) ;  
                                 M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( s z S Q L ,   c ) ;  
                                 c m d . P a r a m e t e r s . A d d ( n e w   M y S q l P a r a m e t e r ( " ? m o n P a r a m e t r e " ,   M y S q l D b T y p e . V a r C h a r ) ) ;  
                                 c m d . P a r a m e t e r s [ 0 ] . V a l u e   =   s z P a r a m ;  
                                 c m d . E x e c u t e N o n Q u e r y ( ) ;  
  
                                 c m d . C o m m a n d T e x t   =   " S E L E C T   *   F R O M   t e s t " ;  
                                 u s i n g   ( M y S q l D a t a R e a d e r   r e a d e r   =   c m d . E x e c u t e R e a d e r ( ) )  
                                 {  
                                         r e a d e r . R e a d ( ) ;  
                                         s t r i n g   s   =   r e a d e r . G e t S t r i n g ( 0 ) ;  
                                         A s s e r t . A r e E q u a l ( s z P a r a m ,   s ) ;  
                                 }  
                         }                                  
                 }  
  
                 [ T e s t ]  
                 p u b l i c   v o i d   B l o b A s U t f 8 ( )  
                 {  
                         e x e c S Q L ( @ " C R E A T E   T A B L E   T e s t ( i n c l u d e _ b l o b   B L O B ,   i n c l u d e _ t i n y b l o b   T I N Y B L O B ,    
                                                 i n c l u d e _ l o n g b l o b   L O N G B L O B ,   e x c l u d e _ t i n y b l o b   T I N Y B L O B ,   e x c l u d e _ b l o b   B L O B ,    
                                                 e x c l u d e _ l o n g b l o b   L O N G B L O B ) " ) ;  
  
                         b y t e [ ]   u t f 8 _ b y t e s   =   n e w   b y t e [ 4 ]   {   0 x f 0 ,   0 x 9 0 ,   0 x 8 0 ,   0 x 8 0   } ;  
                         E n c o d i n g   u t f 8   =   E n c o d i n g . G e t E n c o d i n g ( " U T F - 8 " ) ;  
                         s t r i n g   u t f 8 _ s t r i n g   =   u t f 8 . G e t S t r i n g ( u t f 8 _ b y t e s ,   0 ,   u t f 8 _ b y t e s . L e n g t h ) ;  
  
                         / /   i n s e r t   o u r   U T F - 8   b y t e s   i n t o   t h e   t a b l e  
                         M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " I N S E R T   I N T O   T e s t   V A L U E S   ( ? p 1 ,   ? p 2 ,   ? p 3 ,   ? p 4 ,   ? p 5 ,   ? p 5 ) " ,   c o n n ) ;  
                         c m d . P a r a m e t e r s . A d d W i t h V a l u e ( " ? p 1 " ,   u t f 8 _ b y t e s ) ;  
                         c m d . P a r a m e t e r s . A d d W i t h V a l u e ( " ? p 2 " ,   u t f 8 _ b y t e s ) ;  
                         c m d . P a r a m e t e r s . A d d W i t h V a l u e ( " ? p 3 " ,   u t f 8 _ b y t e s ) ;  
                         c m d . P a r a m e t e r s . A d d W i t h V a l u e ( " ? p 4 " ,   u t f 8 _ b y t e s ) ;  
                         c m d . P a r a m e t e r s . A d d W i t h V a l u e ( " ? p 5 " ,   u t f 8 _ b y t e s ) ;  
                         c m d . P a r a m e t e r s . A d d W i t h V a l u e ( " ? p 6 " ,   u t f 8 _ b y t e s ) ;  
                         c m d . E x e c u t e N o n Q u e r y ( ) ;  
  
                         / /   n o w   c h e c k   t h a t   t h e   o n / o f f   i s   w o r k i n g  
                         s t r i n g   c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; T r e a t   B l o b s   A s   U T F 8 = y e s ; B l o b A s U T F 8 I n c l u d e P a t t e r n = . * " ;  
                         u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
                         {  
                                 c . O p e n ( ) ;  
                                 M y S q l D a t a A d a p t e r   d a   =   n e w   M y S q l D a t a A d a p t e r ( " S E L E C T   *   F R O M   T e s t " ,   c ) ;  
                                 D a t a T a b l e   d t   =   n e w   D a t a T a b l e ( ) ;  
                                 d a . F i l l ( d t ) ;  
                                 f o r e a c h   ( D a t a C o l u m n   c o l   i n   d t . C o l u m n s )  
                                 {  
                                         A s s e r t . A r e E q u a l ( t y p e o f ( s t r i n g ) ,   c o l . D a t a T y p e ) ;  
                                         s t r i n g   s   =   ( s t r i n g ) d t . R o w s [ 0 ] [ 0 ] ;  
                                         b y t e [ ]   b   =   u t f 8 . G e t B y t e s ( s ) ;  
                                         A s s e r t . A r e E q u a l ( u t f 8 _ s t r i n g ,   d t . R o w s [ 0 ] [ c o l . O r d i n a l ] . T o S t r i n g ( ) ) ;  
                                 }  
                         }  
  
                         / /   n o w   c h e c k   t h a t   e x c l u s i o n   w o r k s  
                         c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; T r e a t   B l o b s   A s   U T F 8 = y e s ; B l o b A s U T F 8 E x c l u d e P a t t e r n = e x c l u d e . * " ;  
                         u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
                         {  
                                 c . O p e n ( ) ;  
                                 M y S q l D a t a A d a p t e r   d a   =   n e w   M y S q l D a t a A d a p t e r ( " S E L E C T   *   F R O M   T e s t " ,   c ) ;  
                                 D a t a T a b l e   d t   =   n e w   D a t a T a b l e ( ) ;  
                                 d a . F i l l ( d t ) ;  
                                 f o r e a c h   ( D a t a C o l u m n   c o l   i n   d t . C o l u m n s )  
                                 {  
                                         i f   ( c o l . C o l u m n N a m e . S t a r t s W i t h ( " e x c l u d e " ) )  
                                                 A s s e r t . A r e E q u a l ( t y p e o f ( b y t e [ ] ) ,   c o l . D a t a T y p e ) ;  
                                         e l s e  
                                         {  
                                                 A s s e r t . A r e E q u a l ( t y p e o f ( s t r i n g ) ,   c o l . D a t a T y p e ) ;  
                                                 A s s e r t . A r e E q u a l ( u t f 8 _ s t r i n g ,   d t . R o w s [ 0 ] [ c o l . O r d i n a l ] . T o S t r i n g ( ) ) ;  
                                         }  
                                 }  
                         }  
  
                         / /   n o w   c h e c k   t h a t   i n c l u s i o n   w o r k s  
                         c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; T r e a t   B l o b s   A s   U T F 8 = y e s ; B l o b A s U T F 8 I n c l u d e P a t t e r n = i n c l u d e . * " ;  
                         u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
                         {  
                                 c . O p e n ( ) ;  
                                 M y S q l D a t a A d a p t e r   d a   =   n e w   M y S q l D a t a A d a p t e r ( " S E L E C T   *   F R O M   T e s t " ,   c ) ;  
                                 D a t a T a b l e   d t   =   n e w   D a t a T a b l e ( ) ;  
                                 d a . F i l l ( d t ) ;  
                                 f o r e a c h   ( D a t a C o l u m n   c o l   i n   d t . C o l u m n s )  
                                 {  
                                         i f   ( c o l . C o l u m n N a m e . S t a r t s W i t h ( " i n c l u d e " ) )  
                                         {  
                                                 A s s e r t . A r e E q u a l ( t y p e o f ( s t r i n g ) ,   c o l . D a t a T y p e ) ;  
                                                 A s s e r t . A r e E q u a l ( u t f 8 _ s t r i n g ,   d t . R o w s [ 0 ] [ c o l . O r d i n a l ] . T o S t r i n g ( ) ) ;  
                                         }  
                                         e l s e  
                                                 A s s e r t . A r e E q u a l ( t y p e o f ( b y t e [ ] ) ,   c o l . D a t a T y p e ) ;  
                                 }  
                         }  
                 }  
  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   B u g   # 3 1 1 8 5     	 c o l u m n s   n a m e s   a r e   i n c o r r e c t   w h e n   u s i n g   t h e   ' A S '   c l a u s e   a n d   n a m e   w i t h   a c c e n t s  
                 / / /   B u g   # 3 8 7 2 1     	 G e t O r d i n a l   d o e s n ' t   a c c e p t   c o l u m n   n a m e s   a c c e p t e d   b y   M y S Q L   5 . 0  
 	 	 / / /   < / s u m m a r y >  
 	 	 [ T e s t ]  
 	 	 p u b l i c   v o i d   U T F 8 A s C o l u m n N a m e s ( )  
 	 	 {  
 	 	 	 s t r i n g   c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; c h a r s e t = u t f 8 ; p o o l i n g = f a l s e " ;  
 	 	 	 u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
 	 	 	 {  
 	 	 	 	 c . O p e n ( ) ;  
  
 	 	 	 	 M y S q l D a t a A d a p t e r   d a   =   n e w   M y S q l D a t a A d a p t e r ( " s e l e c t   n o w ( )   a s   ' N u m � r o ' " ,   c ) ;  
 	 	 	 	 D a t a T a b l e   d t   =   n e w   D a t a T a b l e ( ) ;  
 	 	 	 	 d a . F i l l ( d t ) ;  
  
 	 	 	 	 A s s e r t . A r e E q u a l ( " N u m � r o " ,   d t . C o l u m n s [ 0 ] . C o l u m n N a m e ) ;  
  
                                 M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " S E L E C T   N O W ( )   A S   ' N u m � r o ' " ,   c ) ;  
                                 u s i n g   ( M y S q l D a t a R e a d e r   r e a d e r   =   c m d . E x e c u t e R e a d e r ( ) )  
                                 {  
                                         i n t   o r d   =   r e a d e r . G e t O r d i n a l ( " N u m � r o " ) ;  
                                         A s s e r t . A r e E q u a l ( 0 ,   o r d ) ;  
                                 }  
 	 	 	 }  
 	 	 }  
  
 	 	 / / /   < s u m m a r y >  
 	 	 / / /   B u g   # 3 1 1 1 7     	 C o n n e c t o r / N e t   e x c e p t i o n s   d o   n o t   s u p p o r t   s e r v e r   c h a r s e t  
 	 	 / / /   < / s u m m a r y >  
 	 	 [ T e s t ]  
 	 	 p u b l i c   v o i d   N o n L a t i n 1 E x c e p t i o n ( )  
 	 	 {  
 	 	 	 s t r i n g   c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; c h a r s e t = u t f 8 " ;  
  
                         e x e c S Q L ( " C R E A T E   T A B L E   T e s t   ( i d   i n t ) " ) ;  
  
 	 	 	 u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
 	 	 	 {  
 	 	 	 	 c . O p e n ( ) ;  
  
 	 	 	 	 t r y  
 	 	 	 	 {  
 	 	 	 	 	 M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d ( " s e l e c t   ` N u m � r o `   f r o m   T e s t " ,   c ) ;  
 	 	 	 	 	 c m d . E x e c u t e S c a l a r ( ) ;  
 	 	 	 	 }  
 	 	 	 	 c a t c h   ( E x c e p t i o n   e x )  
 	 	 	 	 {  
 	 	 	 	 	 A s s e r t . A r e E q u a l ( " U n k n o w n   c o l u m n   ' N u m � r o '   i n   ' f i e l d   l i s t ' " ,   e x . M e s s a g e ) ;  
 	 	 	 	 }  
 	 	 	 }  
 	 	 }  
  
                 / / /   < s u m m a r y >  
                 / / /   B u g   # 4 0 0 7 6 	 " F u n c t i o n s   R e t u r n   S t r i n g "   o p t i o n   d o e s   n o t   s e t   t h e   p r o p e r   e n c o d i n g   f o r   t h e   s t r i n g  
                 / / /   < / s u m m a r y >  
                 [ T e s t ]  
                 p u b l i c   v o i d   F u n c t i o n R e t u r n s S t r i n g W i t h C h a r S e t ( )  
                 {  
                         s t r i n g   c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; f u n c t i o n s   r e t u r n   s t r i n g = t r u e " ;  
                         u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
                         {  
                                 c . O p e n ( ) ;  
  
                                 M y S q l C o m m a n d   c m d   =   n e w   M y S q l C o m m a n d (  
                                         " S E L E C T   C O N C A T ( ' T r � d g � r d s v � g e n ' ,   1 ) " ,   c ) ;  
  
                                 u s i n g   ( M y S q l D a t a R e a d e r   r e a d e r   =   c m d . E x e c u t e R e a d e r ( ) )  
                                 {  
                                         r e a d e r . R e a d ( ) ;  
                                         A s s e r t . A r e E q u a l ( " T r � d g � r d s v � g e n 1 " ,   r e a d e r . G e t S t r i n g ( 0 ) ) ;  
                                 }  
                         }  
                 }  
  
                 [ T e s t ]  
                 p u b l i c   v o i d   R e s p e c t B i n a r y F l a g s ( )  
                 {  
                         s t r i n g   c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; r e s p e c t   b i n a r y   f l a g s = t r u e " ;  
                         u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
                         {  
                                 c . O p e n ( ) ;  
  
                                 M y S q l D a t a A d a p t e r   d a   =   n e w   M y S q l D a t a A d a p t e r (  
                                         " S E L E C T   C O N C A T ( ' T r � d g � r d s v � g e n ' ,   1 ) " ,   c ) ;  
                                 D a t a T a b l e   d t   =   n e w   D a t a T a b l e ( ) ;  
                                 d a . F i l l ( d t ) ;  
                                 A s s e r t . I s T r u e ( d t . R o w s [ 0 ] [ 0 ]   i s   b y t e [ ] ) ;  
                         }  
                         c o n n S t r   =   G e t C o n n e c t i o n S t r i n g ( t r u e )   +   " ; r e s p e c t   b i n a r y   f l a g s = f a l s e " ;  
                         u s i n g   ( M y S q l C o n n e c t i o n   c   =   n e w   M y S q l C o n n e c t i o n ( c o n n S t r ) )  
                         {  
                                 c . O p e n ( ) ;  
  
                                 M y S q l D a t a A d a p t e r   d a   =   n e w   M y S q l D a t a A d a p t e r (  
                                         " S E L E C T   C O N C A T ( ' T r � d g � r d s v � g e n ' ,   1 ) " ,   c ) ;  
                                 D a t a T a b l e   d t   =   n e w   D a t a T a b l e ( ) ;  
                                 d a . F i l l ( d t ) ;  
                                 A s s e r t . I s T r u e ( d t . R o w s [ 0 ] [ 0 ]   i s   s t r i n g ) ;  
                                 A s s e r t . A r e E q u a l ( " T r � d g � r d s v � g e n 1 " ,   d t . R o w s [ 0 ] [ 0 ] ) ;  
                         }  
                 }  
         }  
 }  
 